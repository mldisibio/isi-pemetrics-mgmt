using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Infrastructure;

/// <summary>No-op implementation of ForNotifyingDataChanges. Use when cache notifications are not needed.</summary>
public sealed class NoOpDataChangeNotifier : ForNotifyingDataChanges
{
    public void NotifyCellChanged(int cellId) { }
    public void NotifyPCStationChanged() { }
    public void NotifyPCToCellMappingChanged(int stationMapId) { }
    public void NotifySwTestChanged(int swTestMapId) { }
    public void NotifySwTestToCellMappingChanged(int swTestMapId) { }
    public void NotifyTLAChanged(string partNo) { }
    public void NotifyTLAToCellMappingChanged(string partNo) { }
}
