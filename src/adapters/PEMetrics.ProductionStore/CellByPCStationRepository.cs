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

    public int Insert(CellByPCStation mapping)
    {
        try
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
            var newStationMapId = (int)outputParam.Value;

            _dataChangeNotifier.NotifyPCToCellMappingChanged(newStationMapId);
            return newStationMapId;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellByPCStation.Insert", ex);
            return -1;
        }
    }

    public bool Update(CellByPCStation mapping)
    {
        try
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

            _dataChangeNotifier.NotifyPCToCellMappingChanged(mapping.StationMapId);
            return true;
        }
        catch (Exception ex)
        {
            _errorNotifier.UnexpectedError("CellByPCStation.Update", ex);
            return false;
        }
    }
}
