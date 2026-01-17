using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.ProductionStore;

/// <summary>SQL Server implementation of ForMappingPCStationToCell.</summary>
public sealed class CellByPCStationRepository : ForMappingPCStationToCell
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly ForNotifyingDataChanges _dataChangeNotifier;

    public CellByPCStationRepository(
        ForCreatingSqlServerConnections connectionFactory,
        ForNotifyingDataCommunicationErrors errorNotifier,
        ForNotifyingDataChanges dataChangeNotifier)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _dataChangeNotifier = dataChangeNotifier ?? throw new ArgumentNullException(nameof(dataChangeNotifier));
    }

    public async Task<int> InsertAsync(CellByPCStation mapping, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellByPCStation_Insert";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@CellId", mapping.CellId));
            sqlCommand.Parameters.Add(new SqlParameter("@PcName", mapping.PcName));
            sqlCommand.Parameters.Add(new SqlParameter("@PcPurpose", (object?)mapping.PcPurpose ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@ActiveFrom", mapping.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
            sqlCommand.Parameters.Add(new SqlParameter("@ActiveTo", (object?)mapping.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@ExtendedName", (object?)mapping.ExtendedName ?? DBNull.Value));

            var outputParam = new SqlParameter("@NewStationMapId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            sqlCommand.Parameters.Add(outputParam);

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            var newStationMapId = (int)outputParam.Value;

            await _dataChangeNotifier.NotifyPCToCellMappingChangedAsync(newStationMapId).ConfigureAwait(false);
            return newStationMapId;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellByPCStation.Insert", ex);
            return -1;
        }
    }

    public async Task<bool> UpdateAsync(CellByPCStation mapping, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.CellByPCStation_Update";
            command.CommandType = CommandType.StoredProcedure;

            var sqlCommand = (SqlCommand)command;
            sqlCommand.Parameters.Add(new SqlParameter("@StationMapId", mapping.StationMapId));
            sqlCommand.Parameters.Add(new SqlParameter("@CellId", mapping.CellId));
            sqlCommand.Parameters.Add(new SqlParameter("@PcName", mapping.PcName));
            sqlCommand.Parameters.Add(new SqlParameter("@PcPurpose", (object?)mapping.PcPurpose ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@ActiveFrom", mapping.ActiveFrom.ToDateTime(TimeOnly.MinValue)));
            sqlCommand.Parameters.Add(new SqlParameter("@ActiveTo", (object?)mapping.ActiveTo?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
            sqlCommand.Parameters.Add(new SqlParameter("@ExtendedName", (object?)mapping.ExtendedName ?? DBNull.Value));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            await _dataChangeNotifier.NotifyPCToCellMappingChangedAsync(mapping.StationMapId).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellByPCStation.Update", ex);
            return false;
        }
    }
}
