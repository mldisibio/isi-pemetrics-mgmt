using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForMappingPartNumberToCells.</summary>
public sealed class CellByPartNoRepository : ForMappingPartNumberToCells
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForMappingCellByPartNoModels _mapper;

    public CellByPartNoRepository(ForCreatingSqlServerConnections connectionFactory, ForMappingCellByPartNoModels mapper)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public ImmutableList<CellByPartNoView> GetAll()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_CellByPartNo ORDER BY PartNo, CellName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellByPartNoView);
    }

    public ImmutableList<CellByPartNo> GetByPartNo(string partNo)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellByPartNo_GetByPartNo";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@PartNo", partNo));

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellByPartNo);
    }

    public void SetMappings(string partNo, IEnumerable<int> cellIds)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellByPartNo_SetMappings";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@PartNo", partNo));
        command.Parameters.Add(CreateIntListParameter("@CellIds", cellIds));

        command.ExecuteNonQuery();
    }

    public void AddMapping(string partNo, int cellId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellByPartNo_AddMapping";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@PartNo", partNo));
        command.Parameters.Add(new SqlParameter("@CellId", cellId));

        command.ExecuteNonQuery();
    }

    public void DeleteMapping(string partNo, int cellId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellByPartNo_DeleteMapping";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@PartNo", partNo));
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
