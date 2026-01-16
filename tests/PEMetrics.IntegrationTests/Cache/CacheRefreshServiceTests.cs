using Microsoft.Extensions.Configuration;
using PEMetrics.DataCache.Configuration;
using PEMetrics.DataCache.Infrastructure;
using PEMetrics.DataCache.Services;
using PEMetrics.IntegrationTests.Fixtures;

namespace PEMetrics.IntegrationTests.Cache;

/// <summary>Tests verifying CacheRefreshService populates DuckDB tables from SQL Server.</summary>
[Collection("SqlServerCollection")]
public sealed class CacheRefreshServiceTests : IAsyncLifetime
{
    readonly SqlServerContainerFixture _sqlFixture;
    readonly TestDuckDbConnectionFactory _duckDbFactory;
    readonly RecordingErrorNotifier _errorNotifier;
    readonly TablePopulationTracker _populationTracker;
    readonly CacheConfiguration _cacheConfig;
    readonly IConfiguration _configuration;
    readonly CacheRefreshService _refreshService;

    public CacheRefreshServiceTests(SqlServerContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
        _duckDbFactory = new TestDuckDbConnectionFactory();
        _errorNotifier = new RecordingErrorNotifier();
        _populationTracker = new TablePopulationTracker();
        _cacheConfig = new CacheConfiguration { MaxParallelPopulation = 2 };

        var odbcConnectionString = OdbcConnectionStringBuilder.ToOdbcConnectionString(sqlFixture.ConnectionString);
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PEMetricsODBC"] = odbcConnectionString
            })
            .Build();

        _refreshService = new CacheRefreshService(
            _duckDbFactory,
            _errorNotifier,
            _populationTracker,
            _cacheConfig,
            _configuration);
    }

    public async Task InitializeAsync()
    {
        // Initialize DuckDB with nanodbc and tables
        await _duckDbFactory.InstallNanodbcAsync();
        await using (var conn = await _duckDbFactory.OpenConnectionAsync().ConfigureAwait(false))
        {
            await DuckDbSchemaCreator.CreateTablesAsync(conn);
        }
    }

    public Task DisposeAsync()
    {
        _refreshService.Dispose();
        _duckDbFactory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task PopulateAllTablesAsync_PopulatesCellTable()
    {
        await _refreshService.PopulateAllTablesAsync();

        await using var connection = await _duckDbFactory.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Cell";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));

        Assert.True(count > 0, "Expected Cell table to be populated");
    }

    [Fact]
    public async Task PopulateAllTablesAsync_PopulatesPCStationTable()
    {
        await _refreshService.PopulateAllTablesAsync();

        await using var connection = await _duckDbFactory.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM PCStation";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));

        Assert.True(count > 0, "Expected PCStation table to be populated");
    }

    [Fact]
    public async Task PopulateAllTablesAsync_PopulatesCellByPCStationTable()
    {
        await _refreshService.PopulateAllTablesAsync();

        await using var connection = await _duckDbFactory.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM CellByPCStation";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));

        Assert.True(count > 0, "Expected CellByPCStation table to be populated");
    }

    [Fact]
    public async Task PopulateAllTablesAsync_PopulatesSwTestMapTable()
    {
        await _refreshService.PopulateAllTablesAsync();

        await using var connection = await _duckDbFactory.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM SwTestMap";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));

        Assert.True(count > 0, "Expected SwTestMap table to be populated");
    }

    [Fact]
    public async Task PopulateAllTablesAsync_PopulatesCellBySwTestTable()
    {
        await _refreshService.PopulateAllTablesAsync();

        await using var connection = await _duckDbFactory.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM CellBySwTest";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));

        Assert.True(count > 0, "Expected CellBySwTest table to be populated");
    }

    [Fact]
    public async Task PopulateAllTablesAsync_PopulatesTLATable()
    {
        await _refreshService.PopulateAllTablesAsync();

        await using var connection = await _duckDbFactory.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM TLA";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));

        Assert.True(count > 0, "Expected TLA table to be populated");
    }

    [Fact]
    public async Task PopulateAllTablesAsync_PopulatesCellByPartNoTable()
    {
        await _refreshService.PopulateAllTablesAsync();

        await using var connection = await _duckDbFactory.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM CellByPartNo";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));

        Assert.True(count > 0, "Expected CellByPartNo table to be populated");
    }

    [Fact]
    public async Task PopulateAllTablesAsync_NoErrors()
    {
        await _refreshService.PopulateAllTablesAsync();

        Assert.False(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)),
            "Expected no errors during cache population");
    }

    [Fact]
    public async Task PopulateAllTablesAsync_CanRefreshMultipleTimes()
    {
        // First population
        await _refreshService.PopulateAllTablesAsync();

        await using var connection = await _duckDbFactory.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Cell";
        var firstCount = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));

        // Second population (should replace data)
        await _refreshService.PopulateAllTablesAsync();

        command.CommandText = "SELECT COUNT(*) FROM Cell";
        var secondCount = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));

        Assert.Equal(firstCount, secondCount);
    }
}
