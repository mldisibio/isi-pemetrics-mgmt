using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using PEMetrics.DataApi.Ports;
using PEMetrics.DataCache.Configuration;
using PEMetrics.DataCache.Infrastructure;

namespace PEMetrics.DataCache.Services;

/// <summary>Background service that populates cache tables from SQL Server using nanodbc.</summary>
public sealed class CacheRefreshService : IDisposable
{
    readonly ForCreatingDuckDbConnections _connectionFactory;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly TablePopulationTracker _populationTracker;
    readonly CacheConfiguration _configuration;
    readonly string _odbcConnectionString;
    readonly Channel<RefreshRequest> _channel;
    readonly CancellationTokenSource _cts;
    Task? _processingTask;
    bool _disposed;

    /// <summary>SQL Server view names mapped to DuckDB tables.</summary>
    static readonly Dictionary<string, string> TableViewMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Cell"] = "mgmt.vw_Cell",
        ["PCStation"] = "mgmt.vw_PCStation",
        ["CellByPCStation"] = "mgmt.vw_CellByPCStation",
        ["SwTestMap"] = "mgmt.vw_SwTestMap",
        ["CellBySwTest"] = "mgmt.vw_CellBySwTest",
        ["CellBySwTestView"] = "mgmt.vw_CellBySwTest",
        ["TLA"] = "mgmt.vw_TLA",
        ["CellByPartNo"] = "mgmt.vw_CellByPartNo",
        ["CellByPartNoView"] = "mgmt.vw_CellByPartNo"
    };

    public CacheRefreshService(
        ForCreatingDuckDbConnections connectionFactory,
        ForNotifyingDataCommunicationErrors errorNotifier,
        TablePopulationTracker populationTracker,
        CacheConfiguration configuration,
        IConfiguration appConfiguration)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _populationTracker = populationTracker ?? throw new ArgumentNullException(nameof(populationTracker));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _odbcConnectionString = appConfiguration.GetConnectionString("PEMetricsODBC")
            ?? throw new InvalidOperationException("PEMetricsODBC connection string not found.");

        _channel = Channel.CreateUnbounded<RefreshRequest>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _cts = new CancellationTokenSource();
    }

    /// <summary>Gets the channel writer for queueing refresh requests.</summary>
    public ChannelWriter<RefreshRequest> Writer => _channel.Writer;

    /// <summary>Starts the background refresh processor.</summary>
    public void Start()
    {
        _processingTask = ProcessRefreshRequestsAsync(_cts.Token);
    }

    /// <summary>Populates all tables during application startup.</summary>
    public async Task PopulateAllTablesAsync(CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(_configuration.MaxParallelPopulation);
        var tables = TableViewMappings.Keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var tasks = tables.Select(async table =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                await PopulateTableAsync(table, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    async Task ProcessRefreshRequestsAsync(CancellationToken cancellationToken)
    {
        await foreach (var request in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            foreach (var table in request.Tables)
            {
                await PopulateTableAsync(table, cancellationToken);
            }
        }
    }

    async Task PopulateTableAsync(string tableName, CancellationToken cancellationToken)
    {
        if (!TableViewMappings.TryGetValue(tableName, out var viewName))
            return;

        using var semaphoreLock = await _populationTracker.AcquireAsync(tableName, cancellationToken);

        try
        {
            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();

            // Delete existing data
            command.CommandText = $"DELETE FROM {tableName}";
            command.ExecuteNonQuery();

            // Use nanodbc odbc_scan to populate from SQL Server view
            command.CommandText = $@"
INSERT INTO {tableName}
SELECT * FROM odbc_scan(
    '{EscapeSqlString(_odbcConnectionString)}',
    '{viewName}'
)";
            await Task.Run(() => command.ExecuteNonQuery(), cancellationToken);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError($"PopulateTable({tableName})", ex);
        }
    }

    static string EscapeSqlString(string value) => value.Replace("'", "''");

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _cts.Cancel();
        _channel.Writer.Complete();

        try
        {
            _processingTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch
        {
            // Ignore cancellation exceptions during shutdown
        }

        _cts.Dispose();
    }
}
