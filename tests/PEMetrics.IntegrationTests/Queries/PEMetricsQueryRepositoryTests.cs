using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.IntegrationTests.Fixtures;
using PEMetrics.ProductionStore;

namespace PEMetrics.IntegrationTests.Queries;

[Collection("SqlServerCollection")]
public sealed class PEMetricsQueryRepositoryTests
{
    readonly SqlServerContainerFixture _fixture;
    readonly PEMetricsQueryRepository _repository;

    public PEMetricsQueryRepositoryTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
        var connectionFactory = new TestConnectionFactory(fixture.ConnectionString);
        var mapper = new DataModelMappers();
        var errorNotifier = new RecordingErrorNotifier();
        _repository = new PEMetricsQueryRepository(connectionFactory, mapper, errorNotifier);
    }

    // Cell Tests

    [Fact]
    public async Task GetCells_ReturnsMultipleRecords()
    {
        var cells = await _repository.GetCellsAsync();

        Assert.True(cells.Count >= 3, "Expected at least 3 seeded cells");
    }

    [Fact]
    public async Task GetCells_IncludesIsActiveFlag()
    {
        var cells = await _repository.GetCellsAsync();

        var activeCell = cells.FirstOrDefault(c => c.CellName == "TestCellA");
        var inactiveCell = cells.FirstOrDefault(c => c.CellName == "TestCellC");

        Assert.NotNull(activeCell);
        Assert.NotNull(inactiveCell);
        Assert.True(activeCell.IsActive);
        Assert.False(inactiveCell.IsActive);
    }

    [Fact]
    public async Task GetCellById_ExistingId_ReturnsCell()
    {
        var cell = await _repository.GetCellByIdAsync(1001);

        Assert.NotNull(cell);
        Assert.Equal("TestCellA", cell.CellName);
    }

    [Fact]
    public async Task GetCellById_NonExistingId_ReturnsNull()
    {
        var cell = await _repository.GetCellByIdAsync(9999);

        Assert.Null(cell);
    }

    // PCStation Tests

    [Fact]
    public async Task GetPCStations_ReturnsMultipleRecords()
    {
        var stations = await _repository.GetPCStationsAsync();

        Assert.True(stations.Count >= 4, "Expected at least 4 seeded PC stations");
    }

    [Fact]
    public async Task SearchPCStations_MatchingPrefix_ReturnsFiltered()
    {
        var stations = await _repository.SearchPCStationsAsync("PC-ALPHA");

        Assert.Equal(2, stations.Count);
        Assert.All(stations, s => Assert.StartsWith("PC-ALPHA", s.PcName));
    }

    [Fact]
    public async Task SearchPCStations_NoMatch_ReturnsEmpty()
    {
        var stations = await _repository.SearchPCStationsAsync("XYZ-NOTFOUND");

        Assert.Empty(stations);
    }

    // CellByPCStation Tests

    [Fact]
    public async Task GetPcToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = await _repository.GetPcToCellMappingsAsync();

        Assert.True(mappings.Count >= 3, "Expected at least 3 seeded PC-to-cell mappings");
    }

    [Fact]
    public async Task GetPcToCellByMapId_ExistingId_ReturnsMapping()
    {
        var mapping = await _repository.GetPcToCellByMapIdAsync(1001);

        Assert.NotNull(mapping);
        Assert.Equal("PC-ALPHA-01", mapping.PcName);
        Assert.Equal("TestCellA", mapping.CellName);
    }

    [Fact]
    public async Task GetPcToCellByMapId_NonExistingId_ReturnsNull()
    {
        var mapping = await _repository.GetPcToCellByMapIdAsync(9999);

        Assert.Null(mapping);
    }

    // SwTestMap Tests

    [Fact]
    public async Task GetSwTests_ReturnsMultipleRecords()
    {
        var tests = await _repository.GetSwTestsAsync();

        Assert.True(tests.Count >= 4, "Expected at least 4 seeded software tests");
    }

    [Fact]
    public async Task GetSwTests_IncludesIsActiveFlag()
    {
        var tests = await _repository.GetSwTestsAsync();

        // T004 has null LastRun, should be active
        var activeTest = tests.FirstOrDefault(t => t.ConfiguredTestId == "T004");
        // T003 has LastRun in 2020, should be inactive
        var inactiveTest = tests.FirstOrDefault(t => t.ConfiguredTestId == "T003");

        Assert.NotNull(activeTest);
        Assert.NotNull(inactiveTest);
        Assert.True(activeTest.IsActive);
        Assert.False(inactiveTest.IsActive);
    }

    [Fact]
    public async Task GetSwTestById_ExistingId_ReturnsTest()
    {
        var test = await _repository.GetSwTestByIdAsync(1001);

        Assert.NotNull(test);
        Assert.Equal("T001", test.ConfiguredTestId);
    }

    [Fact]
    public async Task GetSwTestById_NonExistingId_ReturnsNull()
    {
        var test = await _repository.GetSwTestByIdAsync(9999);

        Assert.Null(test);
    }

    // CellBySwTest Tests

    [Fact]
    public async Task GetSwTestToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = await _repository.GetSwTestToCellMappingsAsync();

        Assert.True(mappings.Count >= 4, "Expected at least 4 seeded sw test-to-cell mappings");
    }

    [Fact]
    public async Task GetSwTestToCellByMapId_ExistingId_ReturnsMappings()
    {
        var mappings = await _repository.GetSwTestToCellByMapIdAsync(1001);

        Assert.True(mappings.Count >= 2, "Expected test 1001 to be mapped to at least 2 cells");
    }

    // TLA Tests

    [Fact]
    public async Task GetTLACatalog_ReturnsMultipleRecords()
    {
        var tlas = await _repository.GetTLACatalogAsync();

        Assert.True(tlas.Count >= 4, "Expected at least 4 seeded TLAs");
    }

    [Fact]
    public async Task GetTLACatalog_IncludesIsUsedFlag()
    {
        var tlas = await _repository.GetTLACatalogAsync();

        // PN-001-A has production test record, should be used
        var usedTla = tlas.FirstOrDefault(t => t.PartNo == "PN-001-A");
        // PN-UNUSED has no production test record, should not be used
        var unusedTla = tlas.FirstOrDefault(t => t.PartNo == "PN-UNUSED");

        Assert.NotNull(usedTla);
        Assert.NotNull(unusedTla);
        Assert.True(usedTla.IsUsed);
        Assert.False(unusedTla.IsUsed);
    }

    [Fact]
    public async Task GetTLAByPartNo_ExistingPartNo_ReturnsTLA()
    {
        var tla = await _repository.GetTLAByPartNoAsync("PN-001-A");

        Assert.NotNull(tla);
        Assert.Equal("Sensors", tla.Family);
    }

    [Fact]
    public async Task GetTLAByPartNo_NonExistingPartNo_ReturnsNull()
    {
        var tla = await _repository.GetTLAByPartNoAsync("NOT-FOUND");

        Assert.Null(tla);
    }

    // CellByPartNo Tests

    [Fact]
    public async Task GetTLAToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = await _repository.GetTLAToCellMappingsAsync();

        Assert.True(mappings.Count >= 4, "Expected at least 4 seeded TLA-to-cell mappings");
    }

    [Fact]
    public async Task GetTLAToCellByPartNo_ExistingPartNo_ReturnsMappings()
    {
        var mappings = await _repository.GetTLAToCellByPartNoAsync("PN-001-A");

        Assert.True(mappings.Count >= 2, "Expected PN-001-A to be mapped to at least 2 cells");
    }
}
