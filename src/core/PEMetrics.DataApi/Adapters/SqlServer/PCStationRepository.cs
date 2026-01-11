using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingPCStations.</summary>
public sealed class PCStationRepository : ForManagingPCStations
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForMappingPCStationModels _mapper;

    public PCStationRepository(ForCreatingSqlServerConnections connectionFactory, ForMappingPCStationModels mapper)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public ImmutableList<PCStation> GetAll()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_PCStation ORDER BY PcName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapPCStation);
    }

    public ImmutableList<PCStation> Search(string prefix)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.PCStation_Search";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@SearchPrefix", prefix));

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapPCStation);
    }

    public void Insert(string pcName)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.PCStation_Insert";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@PcName", pcName));

        command.ExecuteNonQuery();
    }
}
