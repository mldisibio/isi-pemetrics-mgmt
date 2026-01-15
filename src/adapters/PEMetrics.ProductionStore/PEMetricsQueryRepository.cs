using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.ProductionStore;

/// <summary>SQL Server implementation of ForReadingPEMetricsDimensions.</summary>
public sealed class PEMetricsQueryRepository : ForReadingPEMetricsDimensions
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForMappingDataModels _mapper;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;

    public PEMetricsQueryRepository(
        ForCreatingSqlServerConnections connectionFactory,
        ForMappingDataModels mapper,
        ForNotifyingDataCommunicationErrors errorNotifier)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
    }

    public ImmutableList<Cell> GetCells()
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_Cell ORDER BY CellName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCell);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetCells", ex);
            return [];
        }
    }

    public Cell? GetCellById(int cellId)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.Cell_GetById";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@CellId", cellId));

            using var reader = command.ExecuteReader();
            return reader.MapFirstOrDefault(_mapper.MapCell);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetCellById", ex);
            return null;
        }
    }

    public ImmutableList<PCStation> GetPCStations()
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_PCStation ORDER BY PcName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapPCStation);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetPCStations", ex);
            return [];
        }
    }

    public ImmutableList<PCStation> SearchPCStations(string prefix)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.PCStation_Search";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@SearchPrefix", prefix));

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapPCStation);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.SearchPCStations", ex);
            return [];
        }
    }

    public ImmutableList<CellByPCStation> GetPcToCellMappings()
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_CellByPCStation ORDER BY CellName, PcName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellByPCStation);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetPcToCellMappings", ex);
            return [];
        }
    }

    public CellByPCStation? GetPcToCellByMapId(int stationMapId)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellByPCStation_GetById";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@StationMapId", stationMapId));

            using var reader = command.ExecuteReader();
            return reader.MapFirstOrDefault(_mapper.MapCellByPCStation);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetPcToCellByMapId", ex);
            return null;
        }
    }

    public ImmutableList<SwTestMap> GetSwTests()
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_SwTestMap ORDER BY ReportKey, TestName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapSwTestMap);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetSwTests", ex);
            return [];
        }
    }

    public SwTestMap? GetSwTestById(int swTestMapId)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.SwTestMap_GetById";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));

            using var reader = command.ExecuteReader();
            return reader.MapFirstOrDefault(_mapper.MapSwTestMap);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetSwTestById", ex);
            return null;
        }
    }

    public ImmutableList<CellBySwTestView> GetSwTestToCellMappings()
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_CellBySwTest ORDER BY ConfiguredTestId, CellName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellBySwTestView);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetSwTestToCellMappings", ex);
            return [];
        }
    }

    public ImmutableList<CellBySwTest> GetSwTestToCellByMapId(int swTestMapId)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellBySwTest_GetBySwTestMapId";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellBySwTest);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetSwTestToCellByMapId", ex);
            return [];
        }
    }

    public ImmutableList<TLA> GetTLACatalog()
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_TLA ORDER BY PartNo";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapTLA);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetTLACatalog", ex);
            return [];
        }
    }

    public TLA? GetTLAByPartNo(string partNo)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.TLA_GetByPartNo";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@PartNo", partNo));

            using var reader = command.ExecuteReader();
            return reader.MapFirstOrDefault(_mapper.MapTLA);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetTLAByPartNo", ex);
            return null;
        }
    }

    public ImmutableList<CellByPartNoView> GetTLAToCellMappings()
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_CellByPartNo ORDER BY PartNo, CellName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellByPartNoView);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetTLAToCellMappings", ex);
            return [];
        }
    }

    public ImmutableList<CellByPartNo> GetTLAToCellByPartNo(string partNo)
    {
        try
        {
            using var connection = _connectionFactory.OpenConnectionToPEMetrics();
            using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellByPartNo_GetByPartNo";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@PartNo", partNo));

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellByPartNo);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetTLAToCellByPartNo", ex);
            return [];
        }
    }
}
