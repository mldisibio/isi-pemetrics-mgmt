using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.ProductionStore;

/// <summary>SQL Server implementation of ForManagingSwTests.</summary>
public sealed class SwTestMapRepository : ForManagingSwTests
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly ForNotifyingDataChanges _dataChangeNotifier;

    public SwTestMapRepository(
        ForCreatingSqlServerConnections connectionFactory,
        ForNotifyingDataCommunicationErrors errorNotifier,
        ForNotifyingDataChanges dataChangeNotifier)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _dataChangeNotifier = dataChangeNotifier ?? throw new ArgumentNullException(nameof(dataChangeNotifier));
    }

    public async Task<int> InsertAsync(SwTestMap test, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.SwTestMap_Insert";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@ConfiguredTestId", (object?)test.ConfiguredTestId ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@TestApplication", (object?)test.TestApplication ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@TestName", (object?)test.TestName ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@ReportKey", (object?)test.ReportKey ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@TestDirectory", (object?)test.TestDirectory ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@RelativePath", (object?)test.RelativePath ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@LastRun", (object?)test.LastRun?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@Notes", (object?)test.Notes ?? DBNull.Value));

            var outputParam = new SqlParameter("@NewSwTestMapId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            sqlCommand.Parameters.Add(outputParam);

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            var newSwTestMapId = (int)outputParam.Value;

            await _dataChangeNotifier.NotifySwTestChangedAsync(newSwTestMapId).ConfigureAwait(false);
            return newSwTestMapId;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SwTestMap.Insert", ex);
            return -1;
        }
    }

    public async Task<bool> UpdateAsync(SwTestMap test, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.SwTestMap_Update";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@SwTestMapId", test.SwTestMapId));
            sqlCommand.Parameters.Add(new SqlParameter("@ConfiguredTestId", (object?)test.ConfiguredTestId ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@TestApplication", (object?)test.TestApplication ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@TestName", (object?)test.TestName ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@ReportKey", (object?)test.ReportKey ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@TestDirectory", (object?)test.TestDirectory ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@RelativePath", (object?)test.RelativePath ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@LastRun", (object?)test.LastRun?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@Notes", (object?)test.Notes ?? DBNull.Value));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            await _dataChangeNotifier.NotifySwTestChangedAsync(test.SwTestMapId).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SwTestMap.Update", ex);
            return false;
        }
    }
}
