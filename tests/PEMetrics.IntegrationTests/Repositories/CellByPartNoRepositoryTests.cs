using PEMetrics.IntegrationTests.Fixtures;
using PEMetrics.ProductionStore;

namespace PEMetrics.IntegrationTests.Repositories;

[Collection("SqlServerCollection")]
public sealed class CellByPartNoRepositoryTests
{
    readonly SqlServerContainerFixture _fixture;
    readonly TestConnectionFactory _connectionFactory;
    readonly RecordingNotifier _notifier;
    readonly RecordingErrorNotifier _errorNotifier;
    readonly CellByPartNoRepository _repository;

    public CellByPartNoRepositoryTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
        _connectionFactory = new TestConnectionFactory(fixture.ConnectionString);
        _notifier = new RecordingNotifier();
        _errorNotifier = new RecordingErrorNotifier();
        _repository = new CellByPartNoRepository(_connectionFactory, _errorNotifier, _notifier);
    }

    [Fact]
    public async Task SetMappings_ReplaceAll_ReturnsTrue()
    {
        // PN-UNUSED has no mappings initially
        var result = await _repository.SetMappingsAsync("PN-UNUSED", [1001, 1002]);

        Assert.True(result);

        // Cleanup: restore to no mappings
        await _repository.SetMappingsAsync("PN-UNUSED", []);
    }

    [Fact]
    public async Task SetMappings_FiresNotification()
    {
        await _repository.SetMappingsAsync("PN-UNUSED", [1001]);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyTLAToCellMappingChangedAsync), "PN-UNUSED"));

        // Cleanup
        await _repository.SetMappingsAsync("PN-UNUSED", []);
    }

    [Fact]
    public async Task AddMapping_NewMapping_ReturnsTrue()
    {
        // PN-UNUSED doesn't have any mappings
        var result = await _repository.AddMappingAsync("PN-UNUSED", 1003);

        Assert.True(result);

        // Cleanup
        await _repository.DeleteMappingAsync("PN-UNUSED", 1003);
    }

    [Fact]
    public async Task AddMapping_ExistingMapping_ReturnsTrue()
    {
        // PN-001-A already has cell 1001 mapped (idempotent)
        var result = await _repository.AddMappingAsync("PN-001-A", 1001);

        Assert.True(result);
    }

    [Fact]
    public async Task AddMapping_FiresNotification()
    {
        await _repository.AddMappingAsync("PN-UNUSED", 1001);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyTLAToCellMappingChangedAsync), "PN-UNUSED"));

        // Cleanup
        await _repository.DeleteMappingAsync("PN-UNUSED", 1001);
    }

    [Fact]
    public async Task DeleteMapping_ExistingMapping_ReturnsTrue()
    {
        // First add a mapping to delete
        await _repository.AddMappingAsync("PN-UNUSED", 1001);
        _notifier.Clear();

        var result = await _repository.DeleteMappingAsync("PN-UNUSED", 1001);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteMapping_NonExisting_ReturnsTrue()
    {
        // Idempotent delete - mapping doesn't exist
        var result = await _repository.DeleteMappingAsync("PN-UNUSED", 1003);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteMapping_FiresNotification()
    {
        // Add then delete to test notification
        await _repository.AddMappingAsync("PN-UNUSED", 1002);
        _notifier.Clear();

        await _repository.DeleteMappingAsync("PN-UNUSED", 1002);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyTLAToCellMappingChangedAsync), "PN-UNUSED"));
    }

    [Fact]
    public async Task SetMappings_InvalidPartNo_ReturnsFalse()
    {
        var result = await _repository.SetMappingsAsync("NOT-FOUND", [1001]);

        Assert.False(result);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }

    [Fact]
    public async Task AddMapping_InvalidCellId_ReturnsFalse()
    {
        var result = await _repository.AddMappingAsync("PN-001-A", 9999);

        Assert.False(result);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }
}
