using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing PC workstations.</summary>
public interface ForManagingPCStations
{
    /// <summary>Retrieves all PC stations from mgmt.vw_PCStation.</summary>
    IReadOnlyCollection<PCStation> GetAll();

    /// <summary>Searches PC stations by name prefix (for autocomplete).</summary>
    /// <returns>Matching PC stations sorted alphabetically.</returns>
    IReadOnlyCollection<PCStation> Search(string prefix);

    /// <summary>Inserts a new PC station if it does not already exist. Idempotent.</summary>
    void Insert(string pcName);
}
