using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingSwTests.</summary>
public sealed class SwTestMapRepository : ForManagingSwTests
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForMappingDataModels _mapper;

    public SwTestMapRepository(ForCreatingSqlServerConnections connectionFactory, ForMappingDataModels mapper)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public int Insert(SwTestMap test)
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
        return (int)outputParam.Value;
    }

    public void Update(SwTestMap test)
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
    }
}
