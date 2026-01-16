using Microsoft.Data.SqlClient;
using PEMetrics.IntegrationTests.Fixtures;
using PEMetrics.ProductionStore;

namespace PEMetrics.IntegrationTests.Repositories;

[Collection("SqlServerCollection")]
public sealed class PCStationRepositoryTests : IDisposable
{
    readonly SqlServerContainerFixture _fixture;
    readonly TestConnectionFactory _connectionFactory;
    readonly RecordingNotifier _notifier;
    readonly RecordingErrorNotifier _errorNotifier;
    readonly PCStationRepository _repository;
    readonly List<string> _insertedStations = [];

    public PCStationRepositoryTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
        _connectionFactory = new TestConnectionFactory(fixture.ConnectionString);
        _notifier = new RecordingNotifier();
        _errorNotifier = new RecordingErrorNotifier();
        _repository = new PCStationRepository(_connectionFactory, _errorNotifier, _notifier);
    }

    public void Dispose()
    {
        // Clean up any inserted test stations
        foreach (var pcName in _insertedStations)
        {
            DeleteTestStation(pcName);
        }
    }

    void DeleteTestStation(string pcName)
    {
        using var connection = new SqlConnection(_fixture.ConnectionString);
        connection.Open();
        using var command = new SqlCommand("DELETE FROM floor.PCStation WHERE PcName = @PcName", connection);
        command.Parameters.AddWithValue("@PcName", pcName);
        command.ExecuteNonQuery();
    }

    [Fact]
    public async Task Insert_NewStation_ReturnsTrue()
    {
        var pcName = $"PC-TEST-{Guid.NewGuid():N}";
        _insertedStations.Add(pcName);

        var result = await _repository.InsertAsync(pcName);

        Assert.True(result);
    }

    [Fact]
    public async Task Insert_NewStation_FiresNotification()
    {
        var pcName = $"PC-TEST-{Guid.NewGuid():N}";
        _insertedStations.Add(pcName);

        await _repository.InsertAsync(pcName);

        Assert.True(_notifier.WasCalled(nameof(_notifier.NotifyPCStationChanged)));
        Assert.Equal(1, _notifier.CallCount(nameof(_notifier.NotifyPCStationChanged)));
    }

    [Fact]
    public async Task Insert_ExistingStation_ReturnsTrue()
    {
        // PC-ALPHA-01 already exists in seed data (idempotent operation)
        var result = await _repository.InsertAsync("PC-ALPHA-01");

        Assert.True(result);
    }

    [Fact]
    public async Task Insert_ExistingStation_StillFiresNotification()
    {
        // Idempotent insert still fires notification
        await _repository.InsertAsync("PC-ALPHA-01");

        Assert.True(_notifier.WasCalled(nameof(_notifier.NotifyPCStationChanged)));
    }
}
