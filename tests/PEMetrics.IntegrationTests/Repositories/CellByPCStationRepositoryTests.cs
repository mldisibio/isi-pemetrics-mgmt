using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Models;
using PEMetrics.IntegrationTests.Fixtures;
using PEMetrics.ProductionStore;

namespace PEMetrics.IntegrationTests.Repositories;

[Collection("SqlServerCollection")]
public sealed class CellByPCStationRepositoryTests : IDisposable
{
    readonly SqlServerContainerFixture _fixture;
    readonly TestConnectionFactory _connectionFactory;
    readonly RecordingNotifier _notifier;
    readonly RecordingErrorNotifier _errorNotifier;
    readonly CellByPCStationRepository _repository;
    readonly List<int> _insertedMapIds = [];

    public CellByPCStationRepositoryTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
        _connectionFactory = new TestConnectionFactory(fixture.ConnectionString);
        _notifier = new RecordingNotifier();
        _errorNotifier = new RecordingErrorNotifier();
        _repository = new CellByPCStationRepository(_connectionFactory, _errorNotifier, _notifier);
    }

    public void Dispose()
    {
        foreach (var mapId in _insertedMapIds)
        {
            DeleteTestMapping(mapId);
        }
    }

    void DeleteTestMapping(int mapId)
    {
        using var connection = new SqlConnection(_fixture.ConnectionString);
        connection.Open();
        using var command = new SqlCommand("DELETE FROM floor.CellByPCStation WHERE StationMapId = @StationMapId", connection);
        command.Parameters.AddWithValue("@StationMapId", mapId);
        command.ExecuteNonQuery();
    }

    [Fact]
    public async Task Insert_ValidMapping_ReturnsNewStationMapId()
    {
        // PC-UNUSED-01 exists but has no mappings
        var mapping = new CellByPCStation
        {
            StationMapId = 0,
            CellId = 1001,
            CellName = "TestCellA",
            PcName = "PC-UNUSED-01",
            PcPurpose = "Test purpose",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };

        var newId = await _repository.InsertAsync(mapping);
        if (newId > 0) _insertedMapIds.Add(newId);

        Assert.True(newId > 0);
    }

    [Fact]
    public async Task Insert_ValidMapping_FiresNotification()
    {
        var mapping = new CellByPCStation
        {
            StationMapId = 0,
            CellId = 1002,
            CellName = "TestCellB",
            PcName = "PC-UNUSED-01",
            PcPurpose = "Notification test",
            ActiveFrom = DateOnly.Parse("2025-01-01")
        };

        var newId = await _repository.InsertAsync(mapping);
        if (newId > 0) _insertedMapIds.Add(newId);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyPCToCellMappingChanged), newId));
    }

    [Fact]
    public async Task Update_ExistingMapping_ReturnsTrue()
    {
        var mapping = new CellByPCStation
        {
            StationMapId = 1001,
            CellId = 1001,
            CellName = "TestCellA",
            PcName = "PC-ALPHA-01",
            PcPurpose = "Updated purpose",
            ActiveFrom = DateOnly.Parse("2020-01-01")
        };

        var result = await _repository.UpdateAsync(mapping);

        Assert.True(result);

        // Restore original
        var restore = new CellByPCStation
        {
            StationMapId = 1001,
            CellId = 1001,
            CellName = "TestCellA",
            PcName = "PC-ALPHA-01",
            PcPurpose = "Main test station",
            ActiveFrom = DateOnly.Parse("2020-01-01")
        };
        await _repository.UpdateAsync(restore);
    }

    [Fact]
    public async Task Update_ExistingMapping_FiresNotification()
    {
        var mapping = new CellByPCStation
        {
            StationMapId = 1001,
            CellId = 1001,
            CellName = "TestCellA",
            PcName = "PC-ALPHA-01",
            ActiveFrom = DateOnly.Parse("2020-01-01")
        };

        await _repository.UpdateAsync(mapping);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyPCToCellMappingChanged), 1001));
    }

    [Fact]
    public async Task Update_NonExistingMapping_ReturnsFalse()
    {
        var mapping = new CellByPCStation
        {
            StationMapId = 9999,
            CellId = 1001,
            CellName = "TestCellA",
            PcName = "PC-ALPHA-01",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };

        var result = await _repository.UpdateAsync(mapping);

        Assert.False(result);
    }
}
