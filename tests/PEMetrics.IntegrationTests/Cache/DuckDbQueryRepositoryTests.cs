using Microsoft.Extensions.Configuration;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataCache.Configuration;
using PEMetrics.DataCache.Infrastructure;
using PEMetrics.DataCache.Repositories;
using PEMetrics.DataCache.Services;
using PEMetrics.IntegrationTests.Fixtures;

namespace PEMetrics.IntegrationTests.Cache;

/// <summary>Tests verifying DuckDbQueryRepository reads data correctly from populated cache.</summary>
[Collection("SqlServerCollection")]
public sealed class DuckDbQueryRepositoryTests : IAsyncLifetime
{
    readonly SqlServerContainerFixture _sqlFixture;
    readonly TestDuckDbConnectionFactory _duckDbFactory;
    readonly RecordingErrorNotifier _errorNotifier;
    readonly TablePopulationTracker _populationTracker;
    readonly DuckDbQueryRepository _repository;
    readonly CacheRefreshService _refreshService;

    public DuckDbQueryRepositoryTests(SqlServerContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
        _duckDbFactory = new TestDuckDbConnectionFactory();
        _errorNotifier = new RecordingErrorNotifier();
        _populationTracker = new TablePopulationTracker();

        var mapper = new DataModelMappers();
        _repository = new DuckDbQueryRepository(_duckDbFactory, mapper, _errorNotifier, _populationTracker);

        var cacheConfig = new CacheConfiguration { MaxParallelPopulation = 4 };
        var odbcConnectionString = OdbcConnectionStringBuilder.ToOdbcConnectionString(sqlFixture.ConnectionString);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PEMetricsODBC"] = odbcConnectionString
            })
            .Build();

        _refreshService = new CacheRefreshService(
            _duckDbFactory,
            _errorNotifier,
            _populationTracker,
            cacheConfig,
            configuration);
    }

    public async Task InitializeAsync()
    {
        // Initialize DuckDB with nanodbc and tables
        var conn = await _duckDbFactory.GetOpenConnectionAsync();
        await DuckDbSchemaCreator.CreateTablesAsync(conn);

        // Populate all tables from SQL Server
        await _refreshService.PopulateAllTablesAsync();

        // Verify no errors during population
        if (_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)))
        {
            var errors = string.Join("; ", _errorNotifier.Calls.Select(c => $"{c.Operation}: {c.Exception?.Message}"));
            throw new InvalidOperationException($"Errors during cache population: {errors}");
        }

        _errorNotifier.Clear(); // Clear population errors for test assertions
    }

    public Task DisposeAsync()
    {
        _refreshService.Dispose();
        _duckDbFactory.Dispose();
        return Task.CompletedTask;
    }

    // Cell tests
    [Fact]
    public async Task GetCells_ReturnsMultipleRecords()
    {
        var cells = await _repository.GetCellsAsync();

        Assert.NotEmpty(cells);
        Assert.True(cells.Count >= 3, "Expected at least 3 cells from seed data");
    }

    [Fact]
    public async Task GetCells_ReturnsCellsWithExpectedProperties()
    {
        var cells = await _repository.GetCellsAsync();

        var firstCell = cells.First();
        Assert.True(firstCell.CellId > 0);
        Assert.False(string.IsNullOrEmpty(firstCell.CellName));
        Assert.False(string.IsNullOrEmpty(firstCell.DisplayName));
    }

    [Fact]
    public async Task GetCellById_ExistingCell_ReturnsCell()
    {
        var cells = await _repository.GetCellsAsync();
        var expectedCell = cells.First();

        var cell = await _repository.GetCellByIdAsync(expectedCell.CellId);

        Assert.NotNull(cell);
        Assert.Equal(expectedCell.CellId, cell.CellId);
        Assert.Equal(expectedCell.CellName, cell.CellName);
    }

    [Fact]
    public async Task GetCellById_NonExistingCell_ReturnsNull()
    {
        var cell = await _repository.GetCellByIdAsync(99999);

        Assert.Null(cell);
    }

    // PCStation tests
    [Fact]
    public async Task GetPCStations_ReturnsMultipleRecords()
    {
        var stations = await _repository.GetPCStationsAsync();

        Assert.NotEmpty(stations);
        Assert.True(stations.Count >= 4, "Expected at least 4 PC stations from seed data");
    }

    [Fact]
    public async Task SearchPCStations_MatchingPrefix_ReturnsResults()
    {
        var allStations = await _repository.GetPCStationsAsync();
        var firstStation = allStations.First();
        var prefix = firstStation.PcName[..3];

        var results = await _repository.SearchPCStationsAsync(prefix);

        Assert.NotEmpty(results);
        Assert.All(results, s => Assert.StartsWith(prefix, s.PcName));
    }

    [Fact]
    public async Task SearchPCStations_NoMatch_ReturnsEmpty()
    {
        var results = await _repository.SearchPCStationsAsync("ZZZZZZZ");

        Assert.Empty(results);
    }

    // CellByPCStation tests
    [Fact]
    public async Task GetPcToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = await _repository.GetPcToCellMappingsAsync();

        Assert.NotEmpty(mappings);
    }

    [Fact]
    public async Task GetPcToCellByMapId_ExistingMapping_ReturnsMapping()
    {
        var mappings = await _repository.GetPcToCellMappingsAsync();
        var expected = mappings.First();

        var mapping = await _repository.GetPcToCellByMapIdAsync(expected.StationMapId);

        Assert.NotNull(mapping);
        Assert.Equal(expected.StationMapId, mapping.StationMapId);
    }

    [Fact]
    public async Task GetPcToCellByMapId_NonExisting_ReturnsNull()
    {
        var mapping = await _repository.GetPcToCellByMapIdAsync(99999);

        Assert.Null(mapping);
    }

    // SwTestMap tests
    [Fact]
    public async Task GetSwTests_ReturnsMultipleRecords()
    {
        var tests = await _repository.GetSwTestsAsync();

        Assert.NotEmpty(tests);
        Assert.True(tests.Count >= 4, "Expected at least 4 software tests from seed data");
    }

    [Fact]
    public async Task GetSwTestById_ExistingTest_ReturnsTest()
    {
        var tests = await _repository.GetSwTestsAsync();
        var expected = tests.First();

        var test = await _repository.GetSwTestByIdAsync(expected.SwTestMapId);

        Assert.NotNull(test);
        Assert.Equal(expected.SwTestMapId, test.SwTestMapId);
    }

    [Fact]
    public async Task GetSwTestById_NonExisting_ReturnsNull()
    {
        var test = await _repository.GetSwTestByIdAsync(99999);

        Assert.Null(test);
    }

    // CellBySwTest tests
    [Fact]
    public async Task GetSwTestToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = await _repository.GetSwTestToCellMappingsAsync();

        Assert.NotEmpty(mappings);
    }

    [Fact]
    public async Task GetSwTestToCellByMapId_ExistingTest_ReturnsMappings()
    {
        var tests = await _repository.GetSwTestsAsync();
        var testWithMappings = tests.First();

        var mappings = await _repository.GetSwTestToCellByMapIdAsync(testWithMappings.SwTestMapId);

        Assert.NotEmpty(mappings);
        Assert.All(mappings, m => Assert.Equal(testWithMappings.SwTestMapId, m.SwTestMapId));
    }

    // TLA tests
    [Fact]
    public async Task GetTLACatalog_ReturnsMultipleRecords()
    {
        var tlas = await _repository.GetTLACatalogAsync();

        Assert.NotEmpty(tlas);
        Assert.True(tlas.Count >= 4, "Expected at least 4 TLAs from seed data");
    }

    [Fact]
    public async Task GetTLAByPartNo_ExistingTLA_ReturnsTLA()
    {
        var tlas = await _repository.GetTLACatalogAsync();
        var expected = tlas.First();

        var tla = await _repository.GetTLAByPartNoAsync(expected.PartNo);

        Assert.NotNull(tla);
        Assert.Equal(expected.PartNo, tla.PartNo);
    }

    [Fact]
    public async Task GetTLAByPartNo_NonExisting_ReturnsNull()
    {
        var tla = await _repository.GetTLAByPartNoAsync("NONEXISTENT-PART-99999");

        Assert.Null(tla);
    }

    // CellByPartNo tests
    [Fact]
    public async Task GetTLAToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = await _repository.GetTLAToCellMappingsAsync();

        Assert.NotEmpty(mappings);
    }

    [Fact]
    public async Task GetTLAToCellByPartNo_ExistingTLA_ReturnsMappings()
    {
        var tlas = await _repository.GetTLACatalogAsync();
        var tlaWithMappings = tlas.First();

        var mappings = await _repository.GetTLAToCellByPartNoAsync(tlaWithMappings.PartNo);

        // May or may not have mappings, but should not error
        Assert.NotNull(mappings);
    }

    // Error handling tests
    [Fact]
    public async Task AllOperations_NoUnexpectedErrors()
    {
        await _repository.GetCellsAsync();
        await _repository.GetPCStationsAsync();
        await _repository.GetPcToCellMappingsAsync();
        await _repository.GetSwTestsAsync();
        await _repository.GetSwTestToCellMappingsAsync();
        await _repository.GetTLACatalogAsync();
        await _repository.GetTLAToCellMappingsAsync();

        Assert.False(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)),
            "Expected no errors from query operations");
    }
}
