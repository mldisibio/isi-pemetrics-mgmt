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
    public void SetMappings_ReplaceAll_ReturnsTrue()
    {
        // PN-UNUSED has no mappings initially
        var result = _repository.SetMappings("PN-UNUSED", [1001, 1002]);

        Assert.True(result);

        // Cleanup: restore to no mappings
        _repository.SetMappings("PN-UNUSED", []);
    }

    [Fact]
    public void SetMappings_FiresNotification()
    {
        _repository.SetMappings("PN-UNUSED", [1001]);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyTLAToCellMappingChanged), "PN-UNUSED"));

        // Cleanup
        _repository.SetMappings("PN-UNUSED", []);
    }

    [Fact]
    public void AddMapping_NewMapping_ReturnsTrue()
    {
        // PN-UNUSED doesn't have any mappings
        var result = _repository.AddMapping("PN-UNUSED", 1003);

        Assert.True(result);

        // Cleanup
        _repository.DeleteMapping("PN-UNUSED", 1003);
    }

    [Fact]
    public void AddMapping_ExistingMapping_ReturnsTrue()
    {
        // PN-001-A already has cell 1001 mapped (idempotent)
        var result = _repository.AddMapping("PN-001-A", 1001);

        Assert.True(result);
    }

    [Fact]
    public void AddMapping_FiresNotification()
    {
        _repository.AddMapping("PN-UNUSED", 1001);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyTLAToCellMappingChanged), "PN-UNUSED"));

        // Cleanup
        _repository.DeleteMapping("PN-UNUSED", 1001);
    }

    [Fact]
    public void DeleteMapping_ExistingMapping_ReturnsTrue()
    {
        // First add a mapping to delete
        _repository.AddMapping("PN-UNUSED", 1001);
        _notifier.Clear();

        var result = _repository.DeleteMapping("PN-UNUSED", 1001);

        Assert.True(result);
    }

    [Fact]
    public void DeleteMapping_NonExisting_ReturnsTrue()
    {
        // Idempotent delete - mapping doesn't exist
        var result = _repository.DeleteMapping("PN-UNUSED", 1003);

        Assert.True(result);
    }

    [Fact]
    public void DeleteMapping_FiresNotification()
    {
        // Add then delete to test notification
        _repository.AddMapping("PN-UNUSED", 1002);
        _notifier.Clear();

        _repository.DeleteMapping("PN-UNUSED", 1002);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyTLAToCellMappingChanged), "PN-UNUSED"));
    }

    [Fact]
    public void SetMappings_InvalidPartNo_ReturnsFalse()
    {
        var result = _repository.SetMappings("NOT-FOUND", [1001]);

        Assert.False(result);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }

    [Fact]
    public void AddMapping_InvalidCellId_ReturnsFalse()
    {
        var result = _repository.AddMapping("PN-001-A", 9999);

        Assert.False(result);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }
}
