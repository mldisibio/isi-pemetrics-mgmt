using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Models;
using PEMetrics.IntegrationTests.Fixtures;
using PEMetrics.ProductionStore;

namespace PEMetrics.IntegrationTests.Repositories;

[Collection("SqlServerCollection")]
public sealed class SwTestMapRepositoryTests : IDisposable
{
    readonly SqlServerContainerFixture _fixture;
    readonly TestConnectionFactory _connectionFactory;
    readonly RecordingNotifier _notifier;
    readonly RecordingErrorNotifier _errorNotifier;
    readonly SwTestMapRepository _repository;
    readonly List<int> _insertedTestIds = [];

    public SwTestMapRepositoryTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
        _connectionFactory = new TestConnectionFactory(fixture.ConnectionString);
        _notifier = new RecordingNotifier();
        _errorNotifier = new RecordingErrorNotifier();
        _repository = new SwTestMapRepository(_connectionFactory, _errorNotifier, _notifier);
    }

    public void Dispose()
    {
        foreach (var testId in _insertedTestIds)
        {
            DeleteTestSwTestMap(testId);
        }
    }

    void DeleteTestSwTestMap(int testId)
    {
        using var connection = new SqlConnection(_fixture.ConnectionString);
        connection.Open();
        using var command = new SqlCommand("DELETE FROM sw.SwTestMap WHERE SwTestMapId = @SwTestMapId", connection);
        command.Parameters.AddWithValue("@SwTestMapId", testId);
        command.ExecuteNonQuery();
    }

    [Fact]
    public async Task Insert_ValidSwTest_ReturnsNewSwTestMapId()
    {
        var test = new SwTestMap
        {
            SwTestMapId = 0,
            ConfiguredTestId = $"T{Guid.NewGuid():N}"[..16],
            TestApplication = "TestApp",
            TestName = $"Test_{Guid.NewGuid():N}",
            ReportKey = "TEST_KEY"
        };

        var newId = await _repository.InsertAsync(test);
        if (newId > 0) _insertedTestIds.Add(newId);

        Assert.True(newId > 0);
    }

    [Fact]
    public async Task Insert_ValidSwTest_FiresNotification()
    {
        var test = new SwTestMap
        {
            SwTestMapId = 0,
            ConfiguredTestId = $"T{Guid.NewGuid():N}"[..16],
            TestApplication = "TestApp",
            TestName = $"Test_{Guid.NewGuid():N}",
            ReportKey = "NOTIFY_KEY"
        };

        var newId = await _repository.InsertAsync(test);
        if (newId > 0) _insertedTestIds.Add(newId);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifySwTestChanged), newId));
    }

    [Fact]
    public async Task Insert_DuplicateConfiguredTestIdAndName_ReturnsNegativeOne()
    {
        // T001 + "Sensor Calibration Test" already exists in seed data
        var test = new SwTestMap
        {
            SwTestMapId = 0,
            ConfiguredTestId = "T001",
            TestApplication = "CalibrationApp",
            TestName = "Sensor Calibration Test",
            ReportKey = "DUP_KEY"
        };

        var result = await _repository.InsertAsync(test);

        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task Update_ExistingSwTest_ReturnsTrue()
    {
        var test = new SwTestMap
        {
            SwTestMapId = 1001,
            ConfiguredTestId = "T001",
            TestApplication = "CalibrationApp",
            TestName = "Sensor Calibration Test",
            ReportKey = "SENSOR_CAL",
            Notes = "Updated notes"
        };

        var result = await _repository.UpdateAsync(test);

        Assert.True(result);

        // Restore original
        var restore = new SwTestMap
        {
            SwTestMapId = 1001,
            ConfiguredTestId = "T001",
            TestApplication = "CalibrationApp",
            TestName = "Sensor Calibration Test",
            ReportKey = "SENSOR_CAL",
            TestDirectory = @"C:\Tests\Calibration",
            RelativePath = "SensorCal",
            LastRun = DateOnly.Parse("2024-06-15"),
            Notes = "Primary calibration"
        };
        await _repository.UpdateAsync(restore);
    }

    [Fact]
    public async Task Update_ExistingSwTest_FiresNotification()
    {
        var test = new SwTestMap
        {
            SwTestMapId = 1002,
            ConfiguredTestId = "T002",
            TestApplication = "ValidationApp",
            TestName = "Final Validation Test",
            ReportKey = "FINAL_VAL"
        };

        await _repository.UpdateAsync(test);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifySwTestChanged), 1002));
    }

    [Fact]
    public async Task Update_NonExistingSwTest_ReturnsFalse()
    {
        var test = new SwTestMap
        {
            SwTestMapId = 9999,
            ConfiguredTestId = "NOTFOUND",
            TestApplication = "TestApp",
            TestName = "NotFound Test"
        };

        var result = await _repository.UpdateAsync(test);

        Assert.False(result);
    }
}
