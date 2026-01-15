namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing PC workstations.</summary>
public interface ForManagingPCStations
{
    /// <summary>Inserts a new PC station if it does not already exist. Idempotent.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool Insert(string pcName);
}
