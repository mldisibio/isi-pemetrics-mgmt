using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.ProductionStore;

/// <summary>SQL Server implementation of ForMappingSwTestsToCells.</summary>
public sealed class CellBySwTestRepository : ForMappingSwTestsToCells
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly ForNotifyingDataChanges _dataChangeNotifier;

    public CellBySwTestRepository(
        ForCreatingSqlServerConnections connectionFactory,
        ForNotifyingDataCommunicationErrors errorNotifier,
        ForNotifyingDataChanges dataChangeNotifier)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _dataChangeNotifier = dataChangeNotifier ?? throw new ArgumentNullException(nameof(dataChangeNotifier));
    }

    public async Task<bool> SetMappingsAsync(int swTestMapId, IEnumerable<int> cellIds, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellBySwTest_SetMappings";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));
            sqlCommand.Parameters.Add(CreateIntListParameter("@CellIds", cellIds));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _dataChangeNotifier.NotifySwTestToCellMappingChanged(swTestMapId);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellBySwTest.SetMappings", ex);
            return false;
        }
    }

    public async Task<bool> AddMappingAsync(int swTestMapId, int cellId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellBySwTest_AddMapping";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));
            sqlCommand.Parameters.Add(new SqlParameter("@CellId", cellId));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _dataChangeNotifier.NotifySwTestToCellMappingChanged(swTestMapId);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellBySwTest.AddMapping", ex);
            return false;
        }
    }

    public async Task<bool> DeleteMappingAsync(int swTestMapId, int cellId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellBySwTest_DeleteMapping";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));
            sqlCommand.Parameters.Add(new SqlParameter("@CellId", cellId));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _dataChangeNotifier.NotifySwTestToCellMappingChanged(swTestMapId);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellBySwTest.DeleteMapping", ex);
            return false;
        }
    }

    static SqlParameter CreateIntListParameter(string parameterName, IEnumerable<int> values)
    {
        var table = new DataTable();
        table.Columns.Add("Value", typeof(int));
        foreach (var value in values)
        {
            table.Rows.Add(value);
        }

        return new SqlParameter(parameterName, SqlDbType.Structured)
        {
            TypeName = "mgmt.IntList",
            Value = table
        };
    }
}
