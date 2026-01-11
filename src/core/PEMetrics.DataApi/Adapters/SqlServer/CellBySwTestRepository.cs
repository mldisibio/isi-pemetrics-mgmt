using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForMappingSwTestsToCells.</summary>
public sealed class CellBySwTestRepository : ForMappingSwTestsToCells
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForMappingCellBySwTestModels _mapper;

    public CellBySwTestRepository(ForCreatingSqlServerConnections connectionFactory, ForMappingCellBySwTestModels mapper)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public ImmutableList<CellBySwTestView> GetAll()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_CellBySwTest ORDER BY ConfiguredTestId, CellName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellBySwTestView);
    }

    public ImmutableList<CellBySwTest> GetBySwTestMapId(int swTestMapId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellBySwTest_GetBySwTestMapId";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellBySwTest);
    }

    public void SetMappings(int swTestMapId, IEnumerable<int> cellIds)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellBySwTest_SetMappings";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));
        command.Parameters.Add(CreateIntListParameter("@CellIds", cellIds));

        command.ExecuteNonQuery();
    }

    public void AddMapping(int swTestMapId, int cellId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellBySwTest_AddMapping";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));
        command.Parameters.Add(new SqlParameter("@CellId", cellId));

        command.ExecuteNonQuery();
    }

    public void DeleteMapping(int swTestMapId, int cellId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellBySwTest_DeleteMapping";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));
        command.Parameters.Add(new SqlParameter("@CellId", cellId));

        command.ExecuteNonQuery();
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
