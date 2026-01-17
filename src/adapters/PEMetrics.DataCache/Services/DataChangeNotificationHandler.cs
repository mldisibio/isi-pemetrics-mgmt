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

    public Task NotifyCellChangedAsync(int cellId)
    {
        // Cell changes cascade to all tables that reference cells
        var (request, completion) = RefreshRequest.ForTablesAwaitable(
            ["Cell", "CellByPCStation", "CellBySwTest", "CellBySwTestView", "CellByPartNo", "CellByPartNoView"],
            $"CellChanged({cellId})");
        _refreshChannel.TryWrite(request);
        return completion;
    }

    public Task NotifyPCStationChangedAsync()
    {
        var (request, completion) = RefreshRequest.ForTableAwaitable("PCStation", "PCStationChanged");
        _refreshChannel.TryWrite(request);
        return completion;
    }

    public Task NotifyPCToCellMappingChangedAsync(int stationMapId)
    {
        var (request, completion) = RefreshRequest.ForTableAwaitable("CellByPCStation", $"PCToCellMappingChanged({stationMapId})");
        _refreshChannel.TryWrite(request);
        return completion;
    }

    public Task NotifySwTestChangedAsync(int swTestMapId)
    {
        // SwTestMap changes cascade to related views
        var (request, completion) = RefreshRequest.ForTablesAwaitable(
            ["SwTestMap", "CellBySwTest", "CellBySwTestView"],
            $"SwTestChanged({swTestMapId})");
        _refreshChannel.TryWrite(request);
        return completion;
    }

    public Task NotifySwTestToCellMappingChangedAsync(int swTestMapId)
    {
        var (request, completion) = RefreshRequest.ForTablesAwaitable(
            ["CellBySwTest", "CellBySwTestView"],
            $"SwTestToCellMappingChanged({swTestMapId})");
        _refreshChannel.TryWrite(request);
        return completion;
    }

    public Task NotifyTLAChangedAsync(string partNo)
    {
        // TLA changes cascade to related views
        var (request, completion) = RefreshRequest.ForTablesAwaitable(
            ["TLA", "CellByPartNo", "CellByPartNoView"],
            $"TLAChanged({partNo})");
        _refreshChannel.TryWrite(request);
        return completion;
    }

    public Task NotifyTLAToCellMappingChangedAsync(string partNo)
    {
        var (request, completion) = RefreshRequest.ForTablesAwaitable(
            ["CellByPartNo", "CellByPartNoView"],
            $"TLAToCellMappingChanged({partNo})");
        _refreshChannel.TryWrite(request);
        return completion;
    }
}
