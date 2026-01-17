using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.ProductionStore;

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

    public async Task<int> InsertAsync(Cell cell, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.Cell_Insert";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@CellName", cell.CellName));
            sqlCommand.Parameters.Add(new SqlParameter("@DisplayName", cell.DisplayName));
            sqlCommand.Parameters.Add(new SqlParameter("@ActiveFrom", cell.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
            sqlCommand.Parameters.Add(new SqlParameter("@ActiveTo", (object?)cell.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@Description", (object?)cell.Description ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@AlternativeNames", (object?)cell.AlternativeNames ?? DBNull.Value));

            var outputParam = new SqlParameter("@NewCellId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            sqlCommand.Parameters.Add(outputParam);

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            var newCellId = (int)outputParam.Value;

            await _dataChangeNotifier.NotifyCellChangedAsync(newCellId).ConfigureAwait(false);
            return newCellId;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("Cell.Insert", ex);
            return -1;
        }
    }

    public async Task<bool> UpdateAsync(Cell cell, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.Cell_Update";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@CellId", cell.CellId));
            sqlCommand.Parameters.Add(new SqlParameter("@CellName", cell.CellName));
            sqlCommand.Parameters.Add(new SqlParameter("@DisplayName", cell.DisplayName));
            sqlCommand.Parameters.Add(new SqlParameter("@ActiveFrom", cell.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
            sqlCommand.Parameters.Add(new SqlParameter("@ActiveTo", (object?)cell.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@Description", (object?)cell.Description ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@AlternativeNames", (object?)cell.AlternativeNames ?? DBNull.Value));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            await _dataChangeNotifier.NotifyCellChangedAsync(cell.CellId).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("Cell.Update", ex);
            return false;
        }
    }
}
