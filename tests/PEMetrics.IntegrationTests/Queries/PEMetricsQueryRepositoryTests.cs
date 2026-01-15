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
    public void GetCells_ReturnsMultipleRecords()
    {
        var cells = _repository.GetCells();

        Assert.True(cells.Count >= 3, "Expected at least 3 seeded cells");
    }

    [Fact]
    public void GetCells_IncludesIsActiveFlag()
    {
        var cells = _repository.GetCells();

        var activeCell = cells.FirstOrDefault(c => c.CellName == "TestCellA");
        var inactiveCell = cells.FirstOrDefault(c => c.CellName == "TestCellC");

        Assert.NotNull(activeCell);
        Assert.NotNull(inactiveCell);
        Assert.True(activeCell.IsActive);
        Assert.False(inactiveCell.IsActive);
    }

    [Fact]
    public void GetCellById_ExistingId_ReturnsCell()
    {
        var cell = _repository.GetCellById(1001);

        Assert.NotNull(cell);
        Assert.Equal("TestCellA", cell.CellName);
    }

    [Fact]
    public void GetCellById_NonExistingId_ReturnsNull()
    {
        var cell = _repository.GetCellById(9999);

        Assert.Null(cell);
    }

    // PCStation Tests

    [Fact]
    public void GetPCStations_ReturnsMultipleRecords()
    {
        var stations = _repository.GetPCStations();

        Assert.True(stations.Count >= 4, "Expected at least 4 seeded PC stations");
    }

    [Fact]
    public void SearchPCStations_MatchingPrefix_ReturnsFiltered()
    {
        var stations = _repository.SearchPCStations("PC-ALPHA");

        Assert.Equal(2, stations.Count);
        Assert.All(stations, s => Assert.StartsWith("PC-ALPHA", s.PcName));
    }

    [Fact]
    public void SearchPCStations_NoMatch_ReturnsEmpty()
    {
        var stations = _repository.SearchPCStations("XYZ-NOTFOUND");

        Assert.Empty(stations);
    }

    // CellByPCStation Tests

    [Fact]
    public void GetPcToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = _repository.GetPcToCellMappings();

        Assert.True(mappings.Count >= 3, "Expected at least 3 seeded PC-to-cell mappings");
    }

    [Fact]
    public void GetPcToCellByMapId_ExistingId_ReturnsMapping()
    {
        var mapping = _repository.GetPcToCellByMapId(1001);

        Assert.NotNull(mapping);
        Assert.Equal("PC-ALPHA-01", mapping.PcName);
        Assert.Equal("TestCellA", mapping.CellName);
    }

    [Fact]
    public void GetPcToCellByMapId_NonExistingId_ReturnsNull()
    {
        var mapping = _repository.GetPcToCellByMapId(9999);

        Assert.Null(mapping);
    }

    // SwTestMap Tests

    [Fact]
    public void GetSwTests_ReturnsMultipleRecords()
    {
        var tests = _repository.GetSwTests();

        Assert.True(tests.Count >= 4, "Expected at least 4 seeded software tests");
    }

    [Fact]
    public void GetSwTests_IncludesIsActiveFlag()
    {
        var tests = _repository.GetSwTests();

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
    public void GetSwTestById_ExistingId_ReturnsTest()
    {
        var test = _repository.GetSwTestById(1001);

        Assert.NotNull(test);
        Assert.Equal("T001", test.ConfiguredTestId);
    }

    [Fact]
    public void GetSwTestById_NonExistingId_ReturnsNull()
    {
        var test = _repository.GetSwTestById(9999);

        Assert.Null(test);
    }

    // CellBySwTest Tests

    [Fact]
    public void GetSwTestToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = _repository.GetSwTestToCellMappings();

        Assert.True(mappings.Count >= 4, "Expected at least 4 seeded sw test-to-cell mappings");
    }

    [Fact]
    public void GetSwTestToCellByMapId_ExistingId_ReturnsMappings()
    {
        var mappings = _repository.GetSwTestToCellByMapId(1001);

        Assert.True(mappings.Count >= 2, "Expected test 1001 to be mapped to at least 2 cells");
    }

    // TLA Tests

    [Fact]
    public void GetTLACatalog_ReturnsMultipleRecords()
    {
        var tlas = _repository.GetTLACatalog();

        Assert.True(tlas.Count >= 4, "Expected at least 4 seeded TLAs");
    }

    [Fact]
    public void GetTLACatalog_IncludesIsUsedFlag()
    {
        var tlas = _repository.GetTLACatalog();

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
    public void GetTLAByPartNo_ExistingPartNo_ReturnsTLA()
    {
        var tla = _repository.GetTLAByPartNo("PN-001-A");

        Assert.NotNull(tla);
        Assert.Equal("Sensors", tla.Family);
    }

    [Fact]
    public void GetTLAByPartNo_NonExistingPartNo_ReturnsNull()
    {
        var tla = _repository.GetTLAByPartNo("NOT-FOUND");

        Assert.Null(tla);
    }

    // CellByPartNo Tests

    [Fact]
    public void GetTLAToCellMappings_ReturnsMultipleRecords()
    {
        var mappings = _repository.GetTLAToCellMappings();

        Assert.True(mappings.Count >= 4, "Expected at least 4 seeded TLA-to-cell mappings");
    }

    [Fact]
    public void GetTLAToCellByPartNo_ExistingPartNo_ReturnsMappings()
    {
        var mappings = _repository.GetTLAToCellByPartNo("PN-001-A");

        Assert.True(mappings.Count >= 2, "Expected PN-001-A to be mapped to at least 2 cells");
    }
}
