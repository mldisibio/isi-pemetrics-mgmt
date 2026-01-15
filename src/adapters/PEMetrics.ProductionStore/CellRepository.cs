using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingCells.</summary>
public sealed class CellRepository : ForManagingCells
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly ForNotifyingDataChanges _dataChangeNotifier;

    public CellRepository(
        ForCreatingSqlServerConnections connectionFactory,
        ForNotifyingDataCommunicationErrors errorNotifier,
        ForNotifyingDataChanges dataChangeNotifier)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _dataChangeNotifier = dataChangeNotifier ?? throw new ArgumentNullException(nameof(dataChangeNotifier));
    }

    public int Insert(Cell cell)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.Cell_Insert";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@CellName", cell.CellName));
            command.Parameters.Add(new SqlParameter("@DisplayName", cell.DisplayName));
            command.Parameters.Add(new SqlParameter("@ActiveFrom", cell.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
            command.Parameters.Add(new SqlParameter("@ActiveTo", (object?)cell.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Description", (object?)cell.Description ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@AlternativeNames", (object?)cell.AlternativeNames ?? DBNull.Value));

            var outputParam = new SqlParameter("@NewCellId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(outputParam);

            command.ExecuteNonQuery();
            var newCellId = (int)outputParam.Value;

            _dataChangeNotifier.NotifyCellChanged(newCellId);
            return newCellId;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("Cell.Insert", ex);
            return -1;
        }
    }

    public bool Update(Cell cell)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.Cell_Update";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@CellId", cell.CellId));
            command.Parameters.Add(new SqlParameter("@CellName", cell.CellName));
            command.Parameters.Add(new SqlParameter("@DisplayName", cell.DisplayName));
            command.Parameters.Add(new SqlParameter("@ActiveFrom", cell.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
            command.Parameters.Add(new SqlParameter("@ActiveTo", (object?)cell.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Description", (object?)cell.Description ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@AlternativeNames", (object?)cell.AlternativeNames ?? DBNull.Value));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifyCellChanged(cell.CellId);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("Cell.Update", ex);
            return false;
        }
    }
}
