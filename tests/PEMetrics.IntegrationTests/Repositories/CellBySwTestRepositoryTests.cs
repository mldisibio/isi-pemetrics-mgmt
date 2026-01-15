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
    public void SetMappings_ReplaceAll_ReturnsTrue()
    {
        // Test 1003 has no mappings initially
        var result = _repository.SetMappings(1003, [1001, 1002]);

        Assert.True(result);

        // Cleanup: restore to no mappings
        _repository.SetMappings(1003, []);
    }

    [Fact]
    public void SetMappings_FiresNotification()
    {
        _repository.SetMappings(1003, [1001]);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifySwTestToCellMappingChanged), 1003));

        // Cleanup
        _repository.SetMappings(1003, []);
    }

    [Fact]
    public void AddMapping_NewMapping_ReturnsTrue()
    {
        // Test 1003 doesn't have cell 1003 mapped
        var result = _repository.AddMapping(1003, 1003);

        Assert.True(result);

        // Cleanup
        _repository.DeleteMapping(1003, 1003);
    }

    [Fact]
    public void AddMapping_ExistingMapping_ReturnsTrue()
    {
        // Test 1001 already has cell 1001 mapped (idempotent)
        var result = _repository.AddMapping(1001, 1001);

        Assert.True(result);
    }

    [Fact]
    public void AddMapping_FiresNotification()
    {
        _repository.AddMapping(1003, 1001);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifySwTestToCellMappingChanged), 1003));

        // Cleanup
        _repository.DeleteMapping(1003, 1001);
    }

    [Fact]
    public void DeleteMapping_ExistingMapping_ReturnsTrue()
    {
        // First add a mapping to delete
        _repository.AddMapping(1004, 1001);
        _notifier.Clear();

        var result = _repository.DeleteMapping(1004, 1001);

        Assert.True(result);
    }

    [Fact]
    public void DeleteMapping_NonExisting_ReturnsTrue()
    {
        // Idempotent delete - mapping doesn't exist
        var result = _repository.DeleteMapping(1003, 1003);

        Assert.True(result);
    }

    [Fact]
    public void DeleteMapping_FiresNotification()
    {
        // Add then delete to test notification
        _repository.AddMapping(1004, 1002);
        _notifier.Clear();

        _repository.DeleteMapping(1004, 1002);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifySwTestToCellMappingChanged), 1004));
    }

    [Fact]
    public void SetMappings_InvalidSwTestMapId_ReturnsFalse()
    {
        var result = _repository.SetMappings(9999, [1001]);

        Assert.False(result);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }

    [Fact]
    public void AddMapping_InvalidCellId_ReturnsFalse()
    {
        var result = _repository.AddMapping(1001, 9999);

        Assert.False(result);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }
}
