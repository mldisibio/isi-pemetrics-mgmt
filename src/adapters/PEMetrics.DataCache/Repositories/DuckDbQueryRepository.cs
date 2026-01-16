using System.Collections.Immutable;
using System.Data.Common;
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

    public async Task<ImmutableList<Cell>> GetCellsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("Cell", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Cell ORDER BY CellName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCell, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetCells", ex);
            return [];
        }
    }

    public async Task<Cell?> GetCellByIdAsync(int cellId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("Cell", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Cell WHERE CellId = ?";
            var param = command.CreateParameter();
            param.Value = cellId;
            command.Parameters.Add(param);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapFirstOrDefaultAsync(_mapper.MapCell, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetCellById", ex);
            return null;
        }
    }

    public async Task<ImmutableList<PCStation>> GetPCStationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("PCStation", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM PCStation ORDER BY PcName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapPCStation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetPCStations", ex);
            return [];
        }
    }

    public async Task<ImmutableList<PCStation>> SearchPCStationsAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("PCStation", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM PCStation WHERE PcName LIKE ? || '%' ORDER BY PcName";
            var param = command.CreateParameter();
            param.Value = prefix ?? "";
            command.Parameters.Add(param);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapPCStation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.SearchPCStations", ex);
            return [];
        }
    }

    public async Task<ImmutableList<CellByPCStation>> GetPcToCellMappingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("CellByPCStation", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellByPCStation ORDER BY CellName, PcName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellByPCStation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetPcToCellMappings", ex);
            return [];
        }
    }

    public async Task<CellByPCStation?> GetPcToCellByMapIdAsync(int stationMapId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("CellByPCStation", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellByPCStation WHERE StationMapId = ?";
            var param = command.CreateParameter();
            param.Value = stationMapId;
            command.Parameters.Add(param);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapFirstOrDefaultAsync(_mapper.MapCellByPCStation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetPcToCellByMapId", ex);
            return null;
        }
    }

    public async Task<ImmutableList<SwTestMap>> GetSwTestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("SwTestMap", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM SwTestMap ORDER BY ReportKey, TestName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapSwTestMap, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetSwTests", ex);
            return [];
        }
    }

    public async Task<SwTestMap?> GetSwTestByIdAsync(int swTestMapId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("SwTestMap", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM SwTestMap WHERE SwTestMapId = ?";
            var param = command.CreateParameter();
            param.Value = swTestMapId;
            command.Parameters.Add(param);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapFirstOrDefaultAsync(_mapper.MapSwTestMap, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetSwTestById", ex);
            return null;
        }
    }

    public async Task<ImmutableList<CellBySwTestView>> GetSwTestToCellMappingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("CellBySwTestView", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellBySwTestView ORDER BY ConfiguredTestId, CellName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellBySwTestView, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetSwTestToCellMappings", ex);
            return [];
        }
    }

    public async Task<ImmutableList<CellBySwTest>> GetSwTestToCellByMapIdAsync(int swTestMapId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("CellBySwTest", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellBySwTest WHERE SwTestMapId = ?";
            var param = command.CreateParameter();
            param.Value = swTestMapId;
            command.Parameters.Add(param);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellBySwTest, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetSwTestToCellByMapId", ex);
            return [];
        }
    }

    public async Task<ImmutableList<TLA>> GetTLACatalogAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("TLA", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM TLA ORDER BY PartNo";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapTLA, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetTLACatalog", ex);
            return [];
        }
    }

    public async Task<TLA?> GetTLAByPartNoAsync(string partNo, CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("TLA", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM TLA WHERE PartNo = ?";
            var param = command.CreateParameter();
            param.Value = partNo;
            command.Parameters.Add(param);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapFirstOrDefaultAsync(_mapper.MapTLA, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetTLAByPartNo", ex);
            return null;
        }
    }

    public async Task<ImmutableList<CellByPartNoView>> GetTLAToCellMappingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("CellByPartNoView", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellByPartNoView ORDER BY PartNo, CellName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellByPartNoView, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetTLAToCellMappings", ex);
            return [];
        }
    }

    public async Task<ImmutableList<CellByPartNo>> GetTLAToCellByPartNoAsync(string partNo, CancellationToken cancellationToken = default)
    {
        try
        {
            await _populationTracker.WaitForTableAsync("CellByPartNo", cancellationToken).ConfigureAwait(false);

            await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM CellByPartNo WHERE PartNo = ?";
            var param = command.CreateParameter();
            param.Value = partNo;
            command.Parameters.Add(param);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellByPartNo, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("DuckDb.GetTLAToCellByPartNo", ex);
            return [];
        }
    }
}
