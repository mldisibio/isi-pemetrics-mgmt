using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingPartNumbers.</summary>
public sealed class TLARepository : ForManagingPartNumbers
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly ForNotifyingDataChanges _dataChangeNotifier;

    public TLARepository(
        ForCreatingSqlServerConnections connectionFactory,
        ForNotifyingDataCommunicationErrors errorNotifier,
        ForNotifyingDataChanges dataChangeNotifier)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _dataChangeNotifier = dataChangeNotifier ?? throw new ArgumentNullException(nameof(dataChangeNotifier));
    }

    public bool Insert(TLA tla)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.TLA_Insert";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@PartNo", tla.PartNo));
            command.Parameters.Add(new SqlParameter("@Family", (object?)tla.Family ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Subfamily", (object?)tla.Subfamily ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@ServiceGroup", (object?)tla.ServiceGroup ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FormalDescription", (object?)tla.FormalDescription ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Description", (object?)tla.Description ?? DBNull.Value));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifyTLAChanged(tla.PartNo);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("TLA.Insert", ex);
            return false;
        }
    }

    public bool Update(TLA tla)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.TLA_Update";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@PartNo", tla.PartNo));
            command.Parameters.Add(new SqlParameter("@Family", (object?)tla.Family ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Subfamily", (object?)tla.Subfamily ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@ServiceGroup", (object?)tla.ServiceGroup ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@FormalDescription", (object?)tla.FormalDescription ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Description", (object?)tla.Description ?? DBNull.Value));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifyTLAChanged(tla.PartNo);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("TLA.Update", ex);
            return false;
        }
    }

    public bool Delete(string partNo)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.TLA_Delete";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@PartNo", partNo));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifyTLAChanged(partNo);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("TLA.Delete", ex);
            return false;
        }
    }
}
