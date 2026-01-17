using System.Data;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.ProductionStore;

/// <summary>SQL Server implementation of ForManagingPCStations.</summary>
public sealed class PCStationRepository : ForManagingPCStations
{
    readonly ForCreatingSqlServerConnections _connectionFactory;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;
    readonly ForNotifyingDataChanges _dataChangeNotifier;

    public PCStationRepository(
        ForCreatingSqlServerConnections connectionFactory,
        ForNotifyingDataCommunicationErrors errorNotifier,
        ForNotifyingDataChanges dataChangeNotifier)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
        _dataChangeNotifier = dataChangeNotifier ?? throw new ArgumentNullException(nameof(dataChangeNotifier));
    }

    public async Task<bool> InsertAsync(string pcName, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionToPEMetricsAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "mgmt.PCStation_Insert";
            command.CommandType = CommandType.StoredProcedure;
            ((SqlCommand)command).Parameters.Add(new SqlParameter("@PcName", pcName));

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            await _dataChangeNotifier.NotifyPCStationChangedAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("PCStation.Insert", ex);
            return false;
        }
    }
}
