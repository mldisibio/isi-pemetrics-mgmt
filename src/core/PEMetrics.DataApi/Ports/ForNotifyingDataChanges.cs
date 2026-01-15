namespace PEMetrics.DataApi.Ports;

/// <summary>Port for publishing data change notifications to subscribers (e.g., cache refresh).</summary>
public interface ForNotifyingDataChanges
{
    /// <summary>Notifies that a cell was inserted or updated.</summary>
    void NotifyCellChanged(int cellId);

    /// <summary>Notifies that a PC station was inserted.</summary>
    void NotifyPCStationChanged();

    /// <summary>Notifies that a PC-to-cell mapping was inserted or updated.</summary>
    void NotifyPCToCellMappingChanged(int stationMapId);

    /// <summary>Notifies that a software test was inserted or updated.</summary>
    void NotifySwTestChanged(int swTestMapId);

    /// <summary>Notifies that software test to cell mappings were modified.</summary>
    void NotifySwTestToCellMappingChanged(int swTestMapId);

    /// <summary>Notifies that a TLA was inserted, updated, or deleted.</summary>
    void NotifyTLAChanged(string partNo);

    /// <summary>Notifies that TLA to cell mappings were modified.</summary>
    void NotifyTLAToCellMappingChanged(string partNo);
}
