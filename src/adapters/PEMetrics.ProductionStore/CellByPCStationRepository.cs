using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForMappingPCStationToCell.</summary>
public sealed class CellByPCStationRepository : ForMappingPCStationToCell
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForMappingCellByPCStationModels _mapper;

    public CellByPCStationRepository(ForCreatingSqlServerConnections connectionFactory, ForMappingCellByPCStationModels mapper)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public ImmutableList<CellByPCStation> GetAll()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_CellByPCStation ORDER BY CellName, PcName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellByPCStation);
    }

    public CellByPCStation? GetById(int stationMapId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellByPCStation_GetById";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@StationMapId", stationMapId));

        using var reader = command.ExecuteReader();
        return reader.MapFirstOrDefault(_mapper.MapCellByPCStation);
    }

    public int Insert(CellByPCStation mapping)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellByPCStation_Insert";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@CellId", mapping.CellId));
        command.Parameters.Add(new SqlParameter("@PcName", mapping.PcName));
        command.Parameters.Add(new SqlParameter("@PcPurpose", (object?)mapping.PcPurpose ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@ActiveFrom", mapping.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
        command.Parameters.Add(new SqlParameter("@ActiveTo", (object?)mapping.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@ExtendedName", (object?)mapping.ExtendedName ?? DBNull.Value));

        var outputParam = new SqlParameter("@NewStationMapId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        command.Parameters.Add(outputParam);

        command.ExecuteNonQuery();
        return (int)outputParam.Value;
    }

    public void Update(CellByPCStation mapping)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellByPCStation_Update";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@StationMapId", mapping.StationMapId));
        command.Parameters.Add(new SqlParameter("@CellId", mapping.CellId));
        command.Parameters.Add(new SqlParameter("@PcName", mapping.PcName));
        command.Parameters.Add(new SqlParameter("@PcPurpose", (object?)mapping.PcPurpose ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@ActiveFrom", mapping.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
        command.Parameters.Add(new SqlParameter("@ActiveTo", (object?)mapping.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@ExtendedName", (object?)mapping.ExtendedName ?? DBNull.Value));

        command.ExecuteNonQuery();
    }
}
