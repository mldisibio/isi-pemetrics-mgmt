using Microsoft.Data.SqlClient;
using PEMetrics.IntegrationTests.Fixtures;
using PEMetrics.ProductionStore;

namespace PEMetrics.IntegrationTests.Repositories;

[Collection("SqlServerCollection")]
public sealed class CellBySwTestRepositoryTests
{
    readonly SqlServerContainerFixture _fixture;
    readonly TestConnectionFactory _connectionFactory;
    readonly RecordingNotifier _notifier;
    readonly RecordingErrorNotifier _errorNotifier;
    readonly CellBySwTestRepository _repository;

    public CellBySwTestRepositoryTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
        _connectionFactory = new TestConnectionFactory(fixture.ConnectionString);
        _notifier = new RecordingNotifier();
        _errorNotifier = new RecordingErrorNotifier();
        _repository = new CellBySwTestRepository(_connectionFactory, _errorNotifier, _notifier);
    }

    [Fact]
    public async Task SetMappings_ReplaceAll_ReturnsTrue()
    {
        // Test 1003 has no mappings initially
        var result = await _repository.SetMappingsAsync(1003, [1001, 1002]);

        Assert.True(result);

        // Cleanup: restore to no mappings
        await _repository.SetMappingsAsync(1003, []);
    }

    [Fact]
    public async Task SetMappings_FiresNotification()
    {
        await _repository.SetMappingsAsync(1003, [1001]);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifySwTestToCellMappingChanged), 1003));

        // Cleanup
        await _repository.SetMappingsAsync(1003, []);
    }

    [Fact]
    public async Task AddMapping_NewMapping_ReturnsTrue()
    {
        // Test 1003 doesn't have cell 1003 mapped
        var result = await _repository.AddMappingAsync(1003, 1003);

        Assert.True(result);

        // Cleanup
        await _repository.DeleteMappingAsync(1003, 1003);
    }

    [Fact]
    public async Task AddMapping_ExistingMapping_ReturnsTrue()
    {
        // Test 1001 already has cell 1001 mapped (idempotent)
        var result = await _repository.AddMappingAsync(1001, 1001);

        Assert.True(result);
    }

    [Fact]
    public async Task AddMapping_FiresNotification()
    {
        await _repository.AddMappingAsync(1003, 1001);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifySwTestToCellMappingChanged), 1003));

        // Cleanup
        await _repository.DeleteMappingAsync(1003, 1001);
    }

    [Fact]
    public async Task DeleteMapping_ExistingMapping_ReturnsTrue()
    {
        // First add a mapping to delete
        await _repository.AddMappingAsync(1004, 1001);
        _notifier.Clear();

        var result = await _repository.DeleteMappingAsync(1004, 1001);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteMapping_NonExisting_ReturnsTrue()
    {
        // Idempotent delete - mapping doesn't exist
        var result = await _repository.DeleteMappingAsync(1003, 1003);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteMapping_FiresNotification()
    {
        // Add then delete to test notification
        await _repository.AddMappingAsync(1004, 1002);
        _notifier.Clear();

        await _repository.DeleteMappingAsync(1004, 1002);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifySwTestToCellMappingChanged), 1004));
    }

    [Fact]
    public async Task SetMappings_InvalidSwTestMapId_ReturnsFalse()
    {
        var result = await _repository.SetMappingsAsync(9999, [1001]);

        Assert.False(result);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }

    [Fact]
    public async Task AddMapping_InvalidCellId_ReturnsFalse()
    {
        var result = await _repository.AddMappingAsync(1001, 9999);

        Assert.False(result);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }
}
