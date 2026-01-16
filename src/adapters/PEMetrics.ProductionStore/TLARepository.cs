using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.ProductionStore;

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

    public async Task<bool> InsertAsync(TLA tla, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.TLA_Insert";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@PartNo", tla.PartNo));
            sqlCommand.Parameters.Add(new SqlParameter("@Family", (object?)tla.Family ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@Subfamily", (object?)tla.Subfamily ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@ServiceGroup", (object?)tla.ServiceGroup ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@FormalDescription", (object?)tla.FormalDescription ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@Description", (object?)tla.Description ?? DBNull.Value));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _dataChangeNotifier.NotifyTLAChanged(tla.PartNo);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("TLA.Insert", ex);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(TLA tla, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.TLA_Update";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@PartNo", tla.PartNo));
            sqlCommand.Parameters.Add(new SqlParameter("@Family", (object?)tla.Family ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@Subfamily", (object?)tla.Subfamily ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@ServiceGroup", (object?)tla.ServiceGroup ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@FormalDescription", (object?)tla.FormalDescription ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@Description", (object?)tla.Description ?? DBNull.Value));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _dataChangeNotifier.NotifyTLAChanged(tla.PartNo);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("TLA.Update", ex);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string partNo, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.TLA_Delete";
            command.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)command).Parameters.Add(new SqlParameter("@PartNo", partNo));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

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
