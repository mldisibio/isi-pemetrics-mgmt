using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingCells.</summary>
public sealed class CellRepository : ForManagingCells
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForMappingDataModels _mapper;

    public CellRepository(ForCreatingSqlServerConnections connectionFactory, ForMappingDataModels mapper)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public ImmutableList<Cell> GetAll()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_Cell ORDER BY CellName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCell);
    }

    public int Insert(Cell cell)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.Cell_Insert";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@CellName", cell.CellName));
        command.Parameters.Add(new SqlParameter("@DisplayName", cell.DisplayName));
        command.Parameters.Add(new SqlParameter("@ActiveFrom", cell.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
        command.Parameters.Add(new SqlParameter("@ActiveTo", (object?)cell.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Description", (object?)cell.Description ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@AlternativeNames", (object?)cell.AlternativeNames ?? DBNull.Value));

        var outputParam = new SqlParameter("@NewCellId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        command.Parameters.Add(outputParam);

        command.ExecuteNonQuery();
        return (int)outputParam.Value;
    }

    public void Update(Cell cell)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.Cell_Update";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@CellId", cell.CellId));
        command.Parameters.Add(new SqlParameter("@CellName", cell.CellName));
        command.Parameters.Add(new SqlParameter("@DisplayName", cell.DisplayName));
        command.Parameters.Add(new SqlParameter("@ActiveFrom", cell.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
        command.Parameters.Add(new SqlParameter("@ActiveTo", (object?)cell.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Description", (object?)cell.Description ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@AlternativeNames", (object?)cell.AlternativeNames ?? DBNull.Value));

        command.ExecuteNonQuery();
    }
}
