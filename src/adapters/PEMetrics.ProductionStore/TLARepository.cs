using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingPartNumbers.</summary>
public sealed class TLARepository : ForManagingPartNumbers
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForMappingTLAModels _mapper;

    public TLARepository(ForCreatingSqlServerConnections connectionFactory, ForMappingTLAModels mapper)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public ImmutableList<TLA> GetAll()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_TLA ORDER BY PartNo";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapTLA);
    }

    public TLA? GetByPartNo(string partNo)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.TLA_GetByPartNo";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@PartNo", partNo));

        using var reader = command.ExecuteReader();
        return reader.MapFirstOrDefault(_mapper.MapTLA);
    }

    public void Insert(TLA tla)
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
    }

    public void Update(TLA tla)
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
    }

    public void Delete(string partNo)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.TLA_Delete";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@PartNo", partNo));

        command.ExecuteNonQuery();
    }
}
