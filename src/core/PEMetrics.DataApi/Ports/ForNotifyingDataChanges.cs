namespace PEMetrics.DataApi.Ports;

/// <summary>Port for publishing data change notifications to subscribers (e.g., cache refresh).</summary>
/// <remarks>All methods return a Task that completes when the cache has been refreshed.</remarks>
public interface ForNotifyingDataChanges
{
    /// <summary>Notifies that a cell was inserted or updated. Awaitable until cache refresh completes.</summary>
    Task NotifyCellChangedAsync(int cellId);

    /// <summary>Notifies that a PC station was inserted. Awaitable until cache refresh completes.</summary>
    Task NotifyPCStationChangedAsync();

    /// <summary>Notifies that a PC-to-cell mapping was inserted or updated. Awaitable until cache refresh completes.</summary>
    Task NotifyPCToCellMappingChangedAsync(int stationMapId);

    /// <summary>Notifies that a software test was inserted or updated. Awaitable until cache refresh completes.</summary>
    Task NotifySwTestChangedAsync(int swTestMapId);

    /// <summary>Notifies that software test to cell mappings were modified. Awaitable until cache refresh completes.</summary>
    Task NotifySwTestToCellMappingChangedAsync(int swTestMapId);

    /// <summary>Notifies that a TLA was inserted, updated, or deleted. Awaitable until cache refresh completes.</summary>
    Task NotifyTLAChangedAsync(string partNo);

    /// <summary>Notifies that TLA to cell mappings were modified. Awaitable until cache refresh completes.</summary>
    Task NotifyTLAToCellMappingChangedAsync(string partNo);
}
