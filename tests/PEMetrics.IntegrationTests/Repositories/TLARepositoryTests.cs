using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Models;
using PEMetrics.IntegrationTests.Fixtures;
using PEMetrics.ProductionStore;

namespace PEMetrics.IntegrationTests.Repositories;

[Collection("SqlServerCollection")]
public sealed class TLARepositoryTests : IDisposable
{
    readonly SqlServerContainerFixture _fixture;
    readonly TestConnectionFactory _connectionFactory;
    readonly RecordingNotifier _notifier;
    readonly RecordingErrorNotifier _errorNotifier;
    readonly TLARepository _repository;
    readonly List<string> _insertedPartNos = [];

    public TLARepositoryTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
        _connectionFactory = new TestConnectionFactory(fixture.ConnectionString);
        _notifier = new RecordingNotifier();
        _errorNotifier = new RecordingErrorNotifier();
        _repository = new TLARepository(_connectionFactory, _errorNotifier, _notifier);
    }

    public void Dispose()
    {
        foreach (var partNo in _insertedPartNos)
        {
            DeleteTestTLA(partNo);
        }
    }

    void DeleteTestTLA(string partNo)
    {
        using var connection = new SqlConnection(_fixture.ConnectionString);
        connection.Open();
        using var command = new SqlCommand("DELETE FROM product.TLA WHERE PartNo = @PartNo", connection);
        command.Parameters.AddWithValue("@PartNo", partNo);
        command.ExecuteNonQuery();
    }

    [Fact]
    public void Insert_ValidTLA_ReturnsTrue()
    {
        var partNo = $"PN-TEST-{Guid.NewGuid():N}"[..32];
        _insertedPartNos.Add(partNo);

        var tla = new TLA { PartNo = partNo, Family = "TestFamily" };

        var result = _repository.Insert(tla);

        Assert.True(result);
    }

    [Fact]
    public void Insert_ValidTLA_FiresNotification()
    {
        var partNo = $"PN-TEST-{Guid.NewGuid():N}"[..32];
        _insertedPartNos.Add(partNo);

        var tla = new TLA { PartNo = partNo, Family = "TestFamily" };

        _repository.Insert(tla);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyTLAChanged), partNo));
    }

    [Fact]
    public void Insert_DuplicatePartNo_ReturnsFalse()
    {
        // PN-001-A already exists in seed data
        var tla = new TLA { PartNo = "PN-001-A", Family = "Duplicate" };

        var result = _repository.Insert(tla);

        Assert.False(result);
    }

    [Fact]
    public void Update_ExistingTLA_ReturnsTrue()
    {
        var tla = new TLA
        {
            PartNo = "PN-001-B",
            Family = "Sensors",
            Subfamily = "pH Sensors Updated",
            ServiceGroup = "SG-PH"
        };

        var result = _repository.Update(tla);

        Assert.True(result);

        // Restore original
        var restore = new TLA
        {
            PartNo = "PN-001-B",
            Family = "Sensors",
            Subfamily = "pH Sensors",
            ServiceGroup = "SG-PH",
            FormalDescription = "pH Sensor Model B",
            Description = "Premium pH sensor"
        };
        _repository.Update(restore);
    }

    [Fact]
    public void Update_ExistingTLA_FiresNotification()
    {
        var tla = new TLA { PartNo = "PN-001-B", Family = "Sensors" };

        _repository.Update(tla);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyTLAChanged), "PN-001-B"));
    }

    [Fact]
    public void Update_NonExistingTLA_ReturnsFalse()
    {
        var tla = new TLA { PartNo = "NOT-FOUND", Family = "Test" };

        var result = _repository.Update(tla);

        Assert.False(result);
    }

    [Fact]
    public void Delete_UnusedTLA_ReturnsTrue()
    {
        // PN-UNUSED has no production tests or cell mappings
        var result = _repository.Delete("PN-UNUSED");

        Assert.True(result);

        // Restore for other tests
        using var connection = new SqlConnection(_fixture.ConnectionString);
        connection.Open();
        using var command = new SqlCommand(
            "INSERT INTO product.TLA (PartNo, Family, FormalDescription, Description) VALUES ('PN-UNUSED', 'Testing', 'Test Part - Unused', 'Can be deleted')",
            connection);
        command.ExecuteNonQuery();
    }

    [Fact]
    public void Delete_UnusedTLA_FiresNotification()
    {
        // Create a temporary TLA to delete
        var tempPartNo = $"PN-DEL-{Guid.NewGuid():N}"[..32];
        using (var connection = new SqlConnection(_fixture.ConnectionString))
        {
            connection.Open();
            using var cmd = new SqlCommand($"INSERT INTO product.TLA (PartNo) VALUES ('{tempPartNo}')", connection);
            cmd.ExecuteNonQuery();
        }

        _repository.Delete(tempPartNo);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyTLAChanged), tempPartNo));
    }

    [Fact]
    public void Delete_UsedTLA_ReturnsFalse()
    {
        // PN-001-A has production test record
        var result = _repository.Delete("PN-001-A");

        Assert.False(result);
    }

    [Fact]
    public void Delete_TLAWithCellMappings_ReturnsFalse()
    {
        // PN-001-B has cell mappings but no production tests
        var result = _repository.Delete("PN-001-B");

        Assert.False(result);
    }
}
