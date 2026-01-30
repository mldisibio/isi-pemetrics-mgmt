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

    public async Task<ImmutableList<Cell>> GetCellsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_Cell ORDER BY CellName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCell, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetCells", ex);
            return [];
        }
    }

    public async Task<Cell?> GetCellByIdAsync(int cellId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.Cell_GetById";
            command.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)command).Parameters.Add(new SqlParameter("@CellId", cellId));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapFirstOrDefaultAsync(_mapper.MapCell, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetCellById", ex);
            return null;
        }
    }

    public async Task<ImmutableList<PCStation>> GetPCStationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_PCStation ORDER BY PcName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapPCStation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetPCStations", ex);
            return [];
        }
    }

    public async Task<ImmutableList<PCStation>> SearchPCStationsAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.PCStation_Search";
            command.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)command).Parameters.Add(new SqlParameter("@SearchPrefix", prefix));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapPCStation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.SearchPCStations", ex);
            return [];
        }
    }

    public async Task<ImmutableList<CellByPCStation>> GetPcToCellMappingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_CellByPCStation ORDER BY CellName, PcName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellByPCStation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetPcToCellMappings", ex);
            return [];
        }
    }

    public async Task<CellByPCStation?> GetPcToCellByMapIdAsync(int stationMapId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellByPCStation_GetById";
            command.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)command).Parameters.Add(new SqlParameter("@StationMapId", stationMapId));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapFirstOrDefaultAsync(_mapper.MapCellByPCStation, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetPcToCellByMapId", ex);
            return null;
        }
    }

    public async Task<ImmutableList<SwTestMap>> GetSwTestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_SwTestMap ORDER BY ReportKey, TestName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapSwTestMap, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetSwTests", ex);
            return [];
        }
    }

    public async Task<SwTestMap?> GetSwTestByIdAsync(int swTestMapId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.SwTestMap_GetById";
            command.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)command).Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapFirstOrDefaultAsync(_mapper.MapSwTestMap, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetSwTestById", ex);
            return null;
        }
    }

    public async Task<ImmutableList<CellBySwTestView>> GetSwTestToCellMappingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_CellBySwTest ORDER BY ConfiguredTestId, CellName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellBySwTestView, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetSwTestToCellMappings", ex);
            return [];
        }
    }

    public async Task<ImmutableList<CellBySwTest>> GetSwTestToCellByMapIdAsync(int swTestMapId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellBySwTest_GetBySwTestMapId";
            command.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)command).Parameters.Add(new SqlParameter("@SwTestMapId", swTestMapId));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellBySwTest, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetSwTestToCellByMapId", ex);
            return [];
        }
    }

    public async Task<ImmutableList<TLA>> GetTLACatalogAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_TLA ORDER BY PartNo";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapTLA, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetTLACatalog", ex);
            return [];
        }
    }

    public async Task<TLA?> GetTLAByPartNoAsync(string partNo, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.TLA_GetByPartNo";
            command.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)command).Parameters.Add(new SqlParameter("@PartNo", partNo));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapFirstOrDefaultAsync(_mapper.MapTLA, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetTLAByPartNo", ex);
            return null;
        }
    }

    public async Task<ImmutableList<CellByPartNoView>> GetTLAToCellMappingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM mgmt.vw_CellByPartNo ORDER BY PartNo, CellName";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellByPartNoView, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetTLAToCellMappings", ex);
            return [];
        }
    }

    public async Task<ImmutableList<CellByPartNo>> GetTLAToCellByPartNoAsync(string partNo, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellByPartNo_GetByPartNo";
            command.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)command).Parameters.Add(new SqlParameter("@PartNo", partNo));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            return await reader.MapAllAsync(_mapper.MapCellByPartNo, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("SqlServer.GetTLAToCellByPartNo", ex);
            return [];
        }
    }
}
