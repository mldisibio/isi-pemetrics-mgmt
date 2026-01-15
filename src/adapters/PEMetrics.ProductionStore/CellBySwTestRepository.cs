using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

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

    public bool SetMappings(int swTestMapId, IEnumerable<int> cellIds)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellBySwTest_SetMappings";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));
            command.Parameters.Add(CreateIntListParameter("@CellIds", cellIds));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifySwTestToCellMappingChanged(swTestMapId);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellBySwTest.SetMappings", ex);
            return false;
        }
    }

    public bool AddMapping(int swTestMapId, int cellId)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellBySwTest_AddMapping";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));
            command.Parameters.Add(new SqlParameter("@CellId", cellId));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifySwTestToCellMappingChanged(swTestMapId);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellBySwTest.AddMapping", ex);
            return false;
        }
    }

    public bool DeleteMapping(int swTestMapId, int cellId)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellBySwTest_DeleteMapping";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));
            command.Parameters.Add(new SqlParameter("@CellId", cellId));

            command.ExecuteNonQuery();

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
