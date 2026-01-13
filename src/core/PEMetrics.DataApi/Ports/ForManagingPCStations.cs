namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing PC workstations.</summary>
public interface ForManagingPCStations
{
    /// <summary>Inserts a new PC station if it does not already exist. Idempotent.</summary>
    void Insert(string pcName);
}
