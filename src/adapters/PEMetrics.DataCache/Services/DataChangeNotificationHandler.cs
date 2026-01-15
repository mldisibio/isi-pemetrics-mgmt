using System.Threading.Channels;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataCache.Services;

/// <summary>Handles data change notifications and queues cache refresh requests.</summary>
public sealed class DataChangeNotificationHandler : ForNotifyingDataChanges
{
    readonly ChannelWriter<RefreshRequest> _refreshChannel;

    public DataChangeNotificationHandler(ChannelWriter<RefreshRequest> refreshChannel)
    {
        _refreshChannel = refreshChannel ?? throw new ArgumentNullException(nameof(refreshChannel));
    }

    public void NotifyCellChanged(int cellId)
    {
        // Cell changes cascade to all tables that reference cells
        var request = RefreshRequest.ForTables(
            ["Cell", "CellByPCStation", "CellBySwTest", "CellBySwTestView", "CellByPartNo", "CellByPartNoView"],
            $"CellChanged({cellId})");
        _refreshChannel.TryWrite(request);
    }

    public void NotifyPCStationChanged()
    {
        var request = RefreshRequest.ForTable("PCStation", "PCStationChanged");
        _refreshChannel.TryWrite(request);
    }

    public void NotifyPCToCellMappingChanged(int stationMapId)
    {
        var request = RefreshRequest.ForTable("CellByPCStation", $"PCToCellMappingChanged({stationMapId})");
        _refreshChannel.TryWrite(request);
    }

    public void NotifySwTestChanged(int swTestMapId)
    {
        // SwTestMap changes cascade to related views
        var request = RefreshRequest.ForTables(
            ["SwTestMap", "CellBySwTest", "CellBySwTestView"],
            $"SwTestChanged({swTestMapId})");
        _refreshChannel.TryWrite(request);
    }

    public void NotifySwTestToCellMappingChanged(int swTestMapId)
    {
        var request = RefreshRequest.ForTables(
            ["CellBySwTest", "CellBySwTestView"],
            $"SwTestToCellMappingChanged({swTestMapId})");
        _refreshChannel.TryWrite(request);
    }

    public void NotifyTLAChanged(string partNo)
    {
        // TLA changes cascade to related views
        var request = RefreshRequest.ForTables(
            ["TLA", "CellByPartNo", "CellByPartNoView"],
            $"TLAChanged({partNo})");
        _refreshChannel.TryWrite(request);
    }

    public void NotifyTLAToCellMappingChanged(string partNo)
    {
        var request = RefreshRequest.ForTables(
            ["CellByPartNo", "CellByPartNoView"],
            $"TLAToCellMappingChanged({partNo})");
        _refreshChannel.TryWrite(request);
    }
}
