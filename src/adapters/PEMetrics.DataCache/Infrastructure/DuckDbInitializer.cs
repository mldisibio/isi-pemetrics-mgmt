using DuckDB.NET.Data;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Ports;
using PEMetrics.DataCache.Configuration;

namespace PEMetrics.DataCache.Infrastructure;

/// <summary>Initializes DuckDB at startup and handles cleanup on exit.</summary>
public sealed class DuckDbInitializer : IDisposable
{
    readonly CacheConfiguration _configuration;
    readonly CachePathResolver _pathResolver;
    readonly ForCreatingDuckDbConnections _connectionFactory;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly string _resolvedCachePath;
    bool _disposed;

    public DuckDbInitializer(
        CacheConfiguration configuration,
        CachePathResolver pathResolver,
        ForCreatingDuckDbConnections connectionFactory,
        ForNotifyingDataCommunicationErrors errorNotifier)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _resolvedCachePath = _pathResolver.ResolvePath(_configuration.CachePath);
    }

    /// <summary>Initializes DuckDB: installs nanodbc extension and executes init script.</summary>
    /// <returns>True if initialization succeeded, false otherwise.</returns>
    public bool Initialize()
    {
        try
        {
            using var connection = _connectionFactory.OpenConnection() as DuckDBConnection
                ?? throw new InvalidOperationException("Connection factory did not return a DuckDBConnection.");

            // Install and load nanodbc community extension
            InstallNanodbcExtension(connection);

            // Execute initialization SQL if specified
            ExecuteInitScript(connection);

            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDB initialization", ex);
            return false;
        }
    }

    void InstallNanodbcExtension(DuckDBConnection connection)
    {
        using var command = connection.CreateCommand();

        // Install nanodbc from community extensions
        command.CommandText = "INSTALL nanodbc FROM community;";
        command.ExecuteNonQuery();

        // Load the extension
        command.CommandText = "LOAD nanodbc;";
        command.ExecuteNonQuery();
    }

    void ExecuteInitScript(DuckDBConnection connection)
    {
        if (string.IsNullOrWhiteSpace(_configuration.InitSqlPath))
            return;

        var initSqlPath = _pathResolver.ResolvePath(_configuration.InitSqlPath);
        if (!File.Exists(initSqlPath))
            return;

        var initSql = File.ReadAllText(initSqlPath);
        if (string.IsNullOrWhiteSpace(initSql))
            return;

        using var command = connection.CreateCommand();
        command.CommandText = initSql;
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_configuration.DeleteOnExit && File.Exists(_resolvedCachePath))
        {
            try
            {
                File.Delete(_resolvedCachePath);

                // Also delete .wal file if it exists
                var walPath = _resolvedCachePath + ".wal";
                if (File.Exists(walPath))
                    File.Delete(walPath);
            }
            catch
            {
                // Ignore cleanup errors on exit
            }
        }
    }
}
