using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Models;
using PEMetrics.IntegrationTests.Fixtures;
using PEMetrics.ProductionStore;

namespace PEMetrics.IntegrationTests.Repositories;

[Collection("SqlServerCollection")]
public sealed class CellRepositoryTests : IDisposable
{
    readonly SqlServerContainerFixture _fixture;
    readonly TestConnectionFactory _connectionFactory;
    readonly RecordingNotifier _notifier;
    readonly RecordingErrorNotifier _errorNotifier;
    readonly CellRepository _repository;
    readonly List<int> _insertedCellIds = [];

    public CellRepositoryTests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;
        _connectionFactory = new TestConnectionFactory(fixture.ConnectionString);
        _notifier = new RecordingNotifier();
        _errorNotifier = new RecordingErrorNotifier();
        _repository = new CellRepository(_connectionFactory, _errorNotifier, _notifier);
    }

    public void Dispose()
    {
        // Clean up any inserted test cells
        foreach (var cellId in _insertedCellIds)
        {
            DeleteTestCell(cellId);
        }
    }

    void DeleteTestCell(int cellId)
    {
        using var connection = new SqlConnection(_fixture.ConnectionString);
        connection.Open();
        using var command = new SqlCommand("DELETE FROM floor.Cell WHERE CellId = @CellId", connection);
        command.Parameters.AddWithValue("@CellId", cellId);
        command.ExecuteNonQuery();
    }

    [Fact]
    public void Insert_ValidCell_ReturnsNewCellId()
    {
        var cell = new Cell
        {
            CellId = 0,
            CellName = $"TestCell_{Guid.NewGuid():N}",
            DisplayName = $"Test Cell Display {Guid.NewGuid():N}",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };

        var newId = _repository.Insert(cell);
        if (newId > 0) _insertedCellIds.Add(newId);

        Assert.True(newId > 0, "Insert should return a positive CellId");
    }

    [Fact]
    public void Insert_ValidCell_FiresNotification()
    {
        var cell = new Cell
        {
            CellId = 0,
            CellName = $"TestCell_{Guid.NewGuid():N}",
            DisplayName = $"Test Cell Display {Guid.NewGuid():N}",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };

        var newId = _repository.Insert(cell);
        if (newId > 0) _insertedCellIds.Add(newId);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyCellChanged), newId));
        Assert.Equal(1, _notifier.CallCount(nameof(_notifier.NotifyCellChanged)));
    }

    [Fact]
    public void Insert_DuplicateCellName_ReturnsNegativeOne()
    {
        // TestCellA already exists in seed data
        var cell = new Cell
        {
            CellId = 0,
            CellName = "TestCellA",
            DisplayName = "Unique Display Name",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };

        var result = _repository.Insert(cell);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void Insert_DuplicateCellName_DoesNotFireNotification()
    {
        var cell = new Cell
        {
            CellId = 0,
            CellName = "TestCellA",
            DisplayName = "Unique Display Name",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };

        _repository.Insert(cell);

        Assert.Empty(_notifier.Calls);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }

    [Fact]
    public void Update_ExistingCell_ReturnsTrue()
    {
        var cell = new Cell
        {
            CellId = 1001,
            CellName = "TestCellA",
            DisplayName = "Test Cell Alpha Updated",
            ActiveFrom = DateOnly.Parse("2020-01-01"),
            Description = "Updated description"
        };

        var result = _repository.Update(cell);

        Assert.True(result);

        // Restore original
        var restore = new Cell
        {
            CellId = 1001,
            CellName = "TestCellA",
            DisplayName = "Test Cell Alpha",
            ActiveFrom = DateOnly.Parse("2020-01-01"),
            Description = "Active test cell A",
            AlternativeNames = "CellA,Alpha"
        };
        _repository.Update(restore);
    }

    [Fact]
    public void Update_ExistingCell_FiresNotification()
    {
        var cell = new Cell
        {
            CellId = 1001,
            CellName = "TestCellA",
            DisplayName = "Test Cell Alpha",
            ActiveFrom = DateOnly.Parse("2020-01-01"),
            Description = "Notification test"
        };

        _repository.Update(cell);

        Assert.True(_notifier.WasCalledWith(nameof(_notifier.NotifyCellChanged), 1001));
    }

    [Fact]
    public void Update_NonExistingCell_ReturnsFalse()
    {
        var cell = new Cell
        {
            CellId = 9999,
            CellName = "NonExistent",
            DisplayName = "Non Existent Cell",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };

        var result = _repository.Update(cell);

        Assert.False(result);
    }

    [Fact]
    public void Update_NonExistingCell_DoesNotFireNotification()
    {
        var cell = new Cell
        {
            CellId = 9999,
            CellName = "NonExistent",
            DisplayName = "Non Existent Cell",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };

        _repository.Update(cell);

        Assert.Empty(_notifier.Calls);
        Assert.True(_errorNotifier.WasCalled(nameof(_errorNotifier.UnexpectedError)));
    }
}
