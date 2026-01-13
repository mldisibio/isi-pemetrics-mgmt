using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingCells.</summary>
public sealed class PEMetricsQueryRepository : ForReadingPEMetricsDimensions
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForMappingDataModels _mapper;

    public PEMetricsQueryRepository(ForCreatingSqlServerConnections connectionFactory, ForMappingDataModels mapper)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public ImmutableList<Cell> GetCells()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_Cell ORDER BY CellName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCell);
    }

    public Cell? GetCellById(int cellId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.Cell_GetById";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@CellId", cellId));

        using var reader = command.ExecuteReader();
        return reader.MapFirstOrDefault(_mapper.MapCell);
    }

    public ImmutableList<PCStation> GetPCStations()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_PCStation ORDER BY PcName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapPCStation);
    }

    public ImmutableList<PCStation> SearchPCStations(string prefix)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.PCStation_Search";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@SearchPrefix", prefix));

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapPCStation);
    }

    public ImmutableList<CellByPCStation> GetPcToCellMappings()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_CellByPCStation ORDER BY CellName, PcName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellByPCStation);
    }

    public CellByPCStation? GetPcToCellByMapId(int stationMapId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellByPCStation_GetById";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@StationMapId", stationMapId));

        using var reader = command.ExecuteReader();
        return reader.MapFirstOrDefault(_mapper.MapCellByPCStation);
    }

    public ImmutableList<SwTestMap> GetSwTests()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_SwTestMap ORDER BY ReportKey, TestName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapSwTestMap);
    }

    public SwTestMap? GetSwTestById(int swTestMapId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.SwTestMap_GetById";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));

        using var reader = command.ExecuteReader();
        return reader.MapFirstOrDefault(_mapper.MapSwTestMap);
    }

    public ImmutableList<CellBySwTestView> GetSwTestToCellMappings()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_CellBySwTest ORDER BY ConfiguredTestId, CellName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellBySwTestView);
    }

    public ImmutableList<CellBySwTest> GetSwTestToCellByMapId(int swTestMapId)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellBySwTest_GetBySwTestMapId";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellBySwTest);
    }

    public ImmutableList<TLA> GetTLACatalog()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_TLA ORDER BY PartNo";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapTLA);
    }

    public TLA? GetTLAByPartNo(string partNo)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.TLA_GetByPartNo";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@PartNo", partNo));

        using var reader = command.ExecuteReader();
        return reader.MapFirstOrDefault(_mapper.MapTLA);
    }

    public ImmutableList<CellByPartNoView> GetTLAToCellMappings()
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM mgmt.vw_CellByPartNo ORDER BY PartNo, CellName";

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellByPartNoView);
    }

    public ImmutableList<CellByPartNo> GetTLAToCellByPartNo(string partNo)
    {
        using var connection = _connectionFactory.OpenConnectionToPEMetrics();
        using var command = connection.CreateCommand();
        command.CommandText = "mgmt.CellByPartNo_GetByPartNo";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@PartNo", partNo));

        using var reader = command.ExecuteReader();
        return reader.MapAll(_mapper.MapCellByPartNo);
    }
}
