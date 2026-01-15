using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForMappingPartNumberToCells.</summary>
public sealed class CellByPartNoRepository : ForMappingPartNumberToCells
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly ForNotifyingDataChanges _dataChangeNotifier;

    public CellByPartNoRepository(
        ForCreatingSqlServerConnections connectionFactory,
        ForNotifyingDataCommunicationErrors errorNotifier,
        ForNotifyingDataChanges dataChangeNotifier)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _dataChangeNotifier = dataChangeNotifier ?? throw new ArgumentNullException(nameof(dataChangeNotifier));
    }

    public bool SetMappings(string partNo, IEnumerable<int> cellIds)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellByPartNo_SetMappings";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@PartNo", partNo));
            command.Parameters.Add(CreateIntListParameter("@CellIds", cellIds));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifyTLAToCellMappingChanged(partNo);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellByPartNo.SetMappings", ex);
            return false;
        }
    }

    public bool AddMapping(string partNo, int cellId)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellByPartNo_AddMapping";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@PartNo", partNo));
            command.Parameters.Add(new SqlParameter("@CellId", cellId));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifyTLAToCellMappingChanged(partNo);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellByPartNo.AddMapping", ex);
            return false;
        }
    }

    public bool DeleteMapping(string partNo, int cellId)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellByPartNo_DeleteMapping";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@PartNo", partNo));
            command.Parameters.Add(new SqlParameter("@CellId", cellId));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifyTLAToCellMappingChanged(partNo);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellByPartNo.DeleteMapping", ex);
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
