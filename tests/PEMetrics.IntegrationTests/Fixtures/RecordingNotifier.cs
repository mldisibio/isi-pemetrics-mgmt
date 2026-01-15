using PEMetrics.DataApi.Ports;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Recording implementation of ForNotifyingDataChanges for verifying notification contract.</summary>
public sealed class RecordingNotifier : ForNotifyingDataChanges
{
    readonly List<(string Method, object? Arg)> _calls = [];

    public IReadOnlyList<(string Method, object? Arg)> Calls => _calls.AsReadOnly();

    public void Clear() => _calls.Clear();

    public void NotifyCellChanged(int cellId)
        => _calls.Add((nameof(NotifyCellChanged), cellId));

    public void NotifyPCStationChanged()
        => _calls.Add((nameof(NotifyPCStationChanged), null));

    public void NotifyPCToCellMappingChanged(int stationMapId)
        => _calls.Add((nameof(NotifyPCToCellMappingChanged), stationMapId));

    public void NotifySwTestChanged(int swTestMapId)
        => _calls.Add((nameof(NotifySwTestChanged), swTestMapId));

    public void NotifySwTestToCellMappingChanged(int swTestMapId)
        => _calls.Add((nameof(NotifySwTestToCellMappingChanged), swTestMapId));

    public void NotifyTLAChanged(string partNo)
        => _calls.Add((nameof(NotifyTLAChanged), partNo));

    public void NotifyTLAToCellMappingChanged(string partNo)
        => _calls.Add((nameof(NotifyTLAToCellMappingChanged), partNo));

    // Assertion helpers
    public bool WasCalled(string methodName)
        => _calls.Any(c => c.Method == methodName);

    public bool WasCalledWith<T>(string methodName, T arg)
        => _calls.Any(c => c.Method == methodName && Equals(c.Arg, arg));

    public int CallCount(string methodName)
        => _calls.Count(c => c.Method == methodName);
}
