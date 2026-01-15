using System.Collections.Immutable;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;
using PEMetrics.DataCache.Infrastructure;

namespace PEMetrics.DataCache.Repositories;

/// <summary>DuckDB implementation of ForReadingPEMetricsDimensions. Reads from local cache.</summary>
public sealed class DuckDbQueryRepository : ForReadingPEMetricsDimensions
{
    readonly ForCreatingDuckDbConnections _connectionFactory;
    readonly ForMappingDataModels _mapper;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly TablePopulationTracker _populationTracker;

    public DuckDbQueryRepository(
        ForCreatingDuckDbConnections connectionFactory,
        ForMappingDataModels mapper,
        ForNotifyingDataCommunicationErrors errorNotifier,
        TablePopulationTracker populationTracker)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _populationTracker = populationTracker ?? throw new ArgumentNullException(nameof(populationTracker));
    }

    public ImmutableList<Cell> GetCells()
    {
        try
        {
            _populationTracker.WaitForTableAsync("Cell").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Cell ORDER BY CellName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCell);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetCells", ex);
            return [];
        }
    }

    public Cell? GetCellById(int cellId)
    {
        try
        {
            _populationTracker.WaitForTableAsync("Cell").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Cell WHERE CellId = ?";
            var param = command.CreateParameter();
            param.Value = cellId;
            command.Parameters.Add(param);

            using var reader = command.ExecuteReader();
            return reader.MapFirstOrDefault(_mapper.MapCell);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetCellById", ex);
            return null;
        }
    }

    public ImmutableList<PCStation> GetPCStations()
    {
        try
        {
            _populationTracker.WaitForTableAsync("PCStation").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM PCStation ORDER BY PcName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapPCStation);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetPCStations", ex);
            return [];
        }
    }

    public ImmutableList<PCStation> SearchPCStations(string prefix)
    {
        try
        {
            _populationTracker.WaitForTableAsync("PCStation").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM PCStation WHERE PcName LIKE ? || '%' ORDER BY PcName";
            var param = command.CreateParameter();
            param.Value = prefix ?? "";
            command.Parameters.Add(param);

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapPCStation);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.SearchPCStations", ex);
            return [];
        }
    }

    public ImmutableList<CellByPCStation> GetPcToCellMappings()
    {
        try
        {
            _populationTracker.WaitForTableAsync("CellByPCStation").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellByPCStation ORDER BY CellName, PcName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellByPCStation);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetPcToCellMappings", ex);
            return [];
        }
    }

    public CellByPCStation? GetPcToCellByMapId(int stationMapId)
    {
        try
        {
            _populationTracker.WaitForTableAsync("CellByPCStation").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellByPCStation WHERE StationMapId = ?";
            var param = command.CreateParameter();
            param.Value = stationMapId;
            command.Parameters.Add(param);

            using var reader = command.ExecuteReader();
            return reader.MapFirstOrDefault(_mapper.MapCellByPCStation);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetPcToCellByMapId", ex);
            return null;
        }
    }

    public ImmutableList<SwTestMap> GetSwTests()
    {
        try
        {
            _populationTracker.WaitForTableAsync("SwTestMap").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM SwTestMap ORDER BY ReportKey, TestName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapSwTestMap);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetSwTests", ex);
            return [];
        }
    }

    public SwTestMap? GetSwTestById(int swTestMapId)
    {
        try
        {
            _populationTracker.WaitForTableAsync("SwTestMap").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM SwTestMap WHERE SwTestMapId = ?";
            var param = command.CreateParameter();
            param.Value = swTestMapId;
            command.Parameters.Add(param);

            using var reader = command.ExecuteReader();
            return reader.MapFirstOrDefault(_mapper.MapSwTestMap);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetSwTestById", ex);
            return null;
        }
    }

    public ImmutableList<CellBySwTestView> GetSwTestToCellMappings()
    {
        try
        {
            _populationTracker.WaitForTableAsync("CellBySwTestView").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellBySwTestView ORDER BY ConfiguredTestId, CellName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellBySwTestView);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetSwTestToCellMappings", ex);
            return [];
        }
    }

    public ImmutableList<CellBySwTest> GetSwTestToCellByMapId(int swTestMapId)
    {
        try
        {
            _populationTracker.WaitForTableAsync("CellBySwTest").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellBySwTest WHERE SwTestMapId = ?";
            var param = command.CreateParameter();
            param.Value = swTestMapId;
            command.Parameters.Add(param);

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellBySwTest);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetSwTestToCellByMapId", ex);
            return [];
        }
    }

    public ImmutableList<TLA> GetTLACatalog()
    {
        try
        {
            _populationTracker.WaitForTableAsync("TLA").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM TLA ORDER BY PartNo";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapTLA);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetTLACatalog", ex);
            return [];
        }
    }

    public TLA? GetTLAByPartNo(string partNo)
    {
        try
        {
            _populationTracker.WaitForTableAsync("TLA").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM TLA WHERE PartNo = ?";
            var param = command.CreateParameter();
            param.Value = partNo;
            command.Parameters.Add(param);

            using var reader = command.ExecuteReader();
            return reader.MapFirstOrDefault(_mapper.MapTLA);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetTLAByPartNo", ex);
            return null;
        }
    }

    public ImmutableList<CellByPartNoView> GetTLAToCellMappings()
    {
        try
        {
            _populationTracker.WaitForTableAsync("CellByPartNoView").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellByPartNoView ORDER BY PartNo, CellName";

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellByPartNoView);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetTLAToCellMappings", ex);
            return [];
        }
    }

    public ImmutableList<CellByPartNo> GetTLAToCellByPartNo(string partNo)
    {
        try
        {
            _populationTracker.WaitForTableAsync("CellByPartNo").GetAwaiter().GetResult();

            using var connection = _connectionFactory.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellByPartNo WHERE PartNo = ?";
            var param = command.CreateParameter();
            param.Value = partNo;
            command.Parameters.Add(param);

            using var reader = command.ExecuteReader();
            return reader.MapAll(_mapper.MapCellByPartNo);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetTLAToCellByPartNo", ex);
            return [];
        }
    }
}
