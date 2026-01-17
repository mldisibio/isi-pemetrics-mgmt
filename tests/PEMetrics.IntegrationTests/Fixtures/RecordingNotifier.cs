using PEMetrics.DataApi.Ports;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Recording implementation of ForNotifyingDataChanges for verifying notification contract.</summary>
public sealed class RecordingNotifier : ForNotifyingDataChanges
{
    readonly List<(string Method, object? Arg)> _calls = [];

    public IReadOnlyList<(string Method, object? Arg)> Calls => _calls.AsReadOnly();

    public void Clear() => _calls.Clear();

    public Task NotifyCellChangedAsync(int cellId)
    {
        _calls.Add((nameof(NotifyCellChangedAsync), cellId));
        return Task.CompletedTask;
    }

    public Task NotifyPCStationChangedAsync()
    {
        _calls.Add((nameof(NotifyPCStationChangedAsync), null));
        return Task.CompletedTask;
    }

    public Task NotifyPCToCellMappingChangedAsync(int stationMapId)
    {
        _calls.Add((nameof(NotifyPCToCellMappingChangedAsync), stationMapId));
        return Task.CompletedTask;
    }

    public Task NotifySwTestChangedAsync(int swTestMapId)
    {
        _calls.Add((nameof(NotifySwTestChangedAsync), swTestMapId));
        return Task.CompletedTask;
    }

    public Task NotifySwTestToCellMappingChangedAsync(int swTestMapId)
    {
        _calls.Add((nameof(NotifySwTestToCellMappingChangedAsync), swTestMapId));
        return Task.CompletedTask;
    }

    public Task NotifyTLAChangedAsync(string partNo)
    {
        _calls.Add((nameof(NotifyTLAChangedAsync), partNo));
        return Task.CompletedTask;
    }

    public Task NotifyTLAToCellMappingChangedAsync(string partNo)
    {
        _calls.Add((nameof(NotifyTLAToCellMappingChangedAsync), partNo));
        return Task.CompletedTask;
    }

    // Assertion helpers
    public bool WasCalled(string methodName)
        => _calls.Any(c => c.Method == methodName);

    public bool WasCalledWith<T>(string methodName, T arg)
        => _calls.Any(c => c.Method == methodName && Equals(c.Arg, arg));

    public int CallCount(string methodName)
        => _calls.Count(c => c.Method == methodName);
}
