using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Infrastructure;

/// <summary>No-op implementation of ForNotifyingDataChanges. Use when cache notifications are not needed.</summary>
public sealed class NoOpDataChangeNotifier : ForNotifyingDataChanges
{
    public Task NotifyCellChangedAsync(int cellId) => Task.CompletedTask;
    public Task NotifyPCStationChangedAsync() => Task.CompletedTask;
    public Task NotifyPCToCellMappingChangedAsync(int stationMapId) => Task.CompletedTask;
    public Task NotifySwTestChangedAsync(int swTestMapId) => Task.CompletedTask;
    public Task NotifySwTestToCellMappingChangedAsync(int swTestMapId) => Task.CompletedTask;
    public Task NotifyTLAChangedAsync(string partNo) => Task.CompletedTask;
    public Task NotifyTLAToCellMappingChangedAsync(string partNo) => Task.CompletedTask;
}
