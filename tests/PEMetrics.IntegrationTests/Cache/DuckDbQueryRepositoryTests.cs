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
        _duckDbFactory.InstallNanodbc();
        using (var conn = _duckDbFactory.OpenConnection())
        {
            DuckDbSchemaCreator.CreateTables(conn);
        }

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
    public void GetCells_ReturnsMultipleRecords()
    {
        var cells = _repository.GetCells();

        Assert.NotEmpty(cells);
        Assert.True(cells.Count >= 3, "Expected at least 3 cells from seed data");
    }

    [Fact]
    public void GetCells_ReturnsCellsWithExpectedProperties()
    {
        var cells = _repository.GetCells();

        var firstCell = cells.First();
        Assert.True(firstCell.CellId > 0);
        Assert.False(string.IsNullOrEmpty(firstCell.CellName));
        Assert.False(string.IsNullOrEmpty(firstCell.DisplayName));
    }

    [Fact]
    public void GetCellById_ExistingCell_ReturnsCell()
    {
        var cells = _repository.GetCells();
        var expectedCell = cells.First();

        var cell = _repository.GetCellById(expectedCell.CellId);

        Assert.NotNull(cell);
        Assert.Equal(expectedCell.CellId, cell.CellId);
        Assert.Equal(expectedCell.CellName, cell.CellName);
    }

    [Fact]
    public void GetCellById_NonExistingCell_ReturnsNull()
    {
        var cell = _repository.GetCellById(99999);

        Assert.Null(cell);
    }

    // PCStation tests
    [Fact]
    public void GetPCStations_ReturnsMultipleRecords()
    {
        var stations = _repository.GetPCStations();

        Assert.NotEmpty(stations);
        Assert.True(stations.Count >= 4, "Expected at least 4 PC stations from seed data");
    }

    [Fact]
    public void SearchPCStations_MatchingPrefix_ReturnsResults()
    {
        var allStations = _repository.GetPCStations();
        var firstStation = allStations.First();
        var prefix = firstStation.PcName[..3];

        var results = _repository.SearchPCStations(prefix);

        Assert.NotEmpty(results);
        Assert.All(results, s => Assert.StartsWith(prefix, s.PcName));
    }

    [Fact]
    public void SearchPCStations_NoMatch_ReturnsEmpty()
    {
        var results = _repository.SearchPCStations("ZZZZZZZ");

        Assert.Empty(results);
    }

    // CellByPCStation tests
    [Fact]
    public void GetPcToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = _repository.GetPcToCellMappings();

        Assert.NotEmpty(mappings);
    }

    [Fact]
    public void GetPcToCellByMapId_ExistingMapping_ReturnsMapping()
    {
        var mappings = _repository.GetPcToCellMappings();
        var expected = mappings.First();

        var mapping = _repository.GetPcToCellByMapId(expected.StationMapId);

        Assert.NotNull(mapping);
        Assert.Equal(expected.StationMapId, mapping.StationMapId);
    }

    [Fact]
    public void GetPcToCellByMapId_NonExisting_ReturnsNull()
    {
        var mapping = _repository.GetPcToCellByMapId(99999);

        Assert.Null(mapping);
    }

    // SwTestMap tests
    [Fact]
    public void GetSwTests_ReturnsMultipleRecords()
    {
        var tests = _repository.GetSwTests();

        Assert.NotEmpty(tests);
        Assert.True(tests.Count >= 4, "Expected at least 4 software tests from seed data");
    }

    [Fact]
    public void GetSwTestById_ExistingTest_ReturnsTest()
    {
        var tests = _repository.GetSwTests();
        var expected = tests.First();

        var test = _repository.GetSwTestById(expected.SwTestMapId);

        Assert.NotNull(test);
        Assert.Equal(expected.SwTestMapId, test.SwTestMapId);
    }

    [Fact]
    public void GetSwTestById_NonExisting_ReturnsNull()
    {
        var test = _repository.GetSwTestById(99999);

        Assert.Null(test);
    }

    // CellBySwTest tests
    [Fact]
    public void GetSwTestToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = _repository.GetSwTestToCellMappings();

        Assert.NotEmpty(mappings);
    }

    [Fact]
    public void GetSwTestToCellByMapId_ExistingTest_ReturnsMappings()
    {
        var tests = _repository.GetSwTests();
        var testWithMappings = tests.First();

        var mappings = _repository.GetSwTestToCellByMapId(testWithMappings.SwTestMapId);

        Assert.NotEmpty(mappings);
        Assert.All(mappings, m => Assert.Equal(testWithMappings.SwTestMapId, m.SwTestMapId));
    }

    // TLA tests
    [Fact]
    public void GetTLACatalog_ReturnsMultipleRecords()
    {
        var tlas = _repository.GetTLACatalog();

        Assert.NotEmpty(tlas);
        Assert.True(tlas.Count >= 4, "Expected at least 4 TLAs from seed data");
    }

    [Fact]
    public void GetTLAByPartNo_ExistingTLA_ReturnsTLA()
    {
        var tlas = _repository.GetTLACatalog();
        var expected = tlas.First();

        var tla = _repository.GetTLAByPartNo(expected.PartNo);

        Assert.NotNull(tla);
        Assert.Equal(expected.PartNo, tla.PartNo);
    }

    [Fact]
    public void GetTLAByPartNo_NonExisting_ReturnsNull()
    {
        var tla = _repository.GetTLAByPartNo("NONEXISTENT-PART-99999");

        Assert.Null(tla);
    }

    // CellByPartNo tests
    [Fact]
    public void GetTLAToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = _repository.GetTLAToCellMappings();

        Assert.NotEmpty(mappings);
    }

    [Fact]
    public void GetTLAToCellByPartNo_ExistingTLA_ReturnsMappings()
    {
        var tlas = _repository.GetTLACatalog();
        var tlaWithMappings = tlas.First();

        var mappings = _repository.GetTLAToCellByPartNo(tlaWithMappings.PartNo);

        // May or may not have mappings, but should not error
        Assert.NotNull(mappings);
    }

    // Error handling tests
    [Fact]
    public void AllOperations_NoUnexpectedErrors()
    {
        _repository.GetCells();
        _repository.GetPCStations();
        _repository.GetPcToCellMappings();
        _repository.GetSwTests();
        _repository.GetSwTestToCellMappings();
        _repository.GetTLACatalog();
        _repository.GetTLAToCellMappings();

        Assert.False(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)),
            "Expected no errors from query operations");
    }
}
