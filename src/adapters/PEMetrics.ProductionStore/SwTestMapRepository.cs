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

    public int Insert(SwTestMap test)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.SwTestMap_Insert";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@ConfiguredTestId", (object?)test.ConfiguredTestId ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@TestApplication", (object?)test.TestApplication ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@TestName", (object?)test.TestName ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@ReportKey", (object?)test.ReportKey ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@TestDirectory", (object?)test.TestDirectory ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@RelativePath", (object?)test.RelativePath ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@LastRun", (object?)test.LastRun?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Notes", (object?)test.Notes ?? DBNull.Value));

            var outputParam = new SqlParameter("@NewSwTestMapId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(outputParam);

            command.ExecuteNonQuery();
            var newSwTestMapId = (int)outputParam.Value;

            _dataChangeNotifier.NotifySwTestChanged(newSwTestMapId);
            return newSwTestMapId;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SwTestMap.Insert", ex);
            return -1;
        }
    }

    public bool Update(SwTestMap test)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.SwTestMap_Update";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@SwTestMapId", test.SwTestMapId));
            command.Parameters.Add(new SqlParameter("@ConfiguredTestId", (object?)test.ConfiguredTestId ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@TestApplication", (object?)test.TestApplication ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@TestName", (object?)test.TestName ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@ReportKey", (object?)test.ReportKey ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@TestDirectory", (object?)test.TestDirectory ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@RelativePath", (object?)test.RelativePath ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@LastRun", (object?)test.LastRun?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Notes", (object?)test.Notes ?? DBNull.Value));

            command.ExecuteNonQuery();

            _dataChangeNotifier.NotifySwTestChanged(test.SwTestMapId);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SwTestMap.Update", ex);
            return false;
        }
    }
}
