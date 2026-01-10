using System.Collections.Immutable;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing PC workstations.</summary>
public interface ForManagingPCStations
{
    /// <summary>Retrieves all PC stations from mgmt.vw_PCStation.</summary>
    ImmutableList<PCStation> GetAll();

    /// <summary>Searches PC stations by name prefix (for autocomplete).</summary>
    /// <returns>Matching PC stations sorted alphabetically.</returns>
    ImmutableList<PCStation> Search(string prefix);

    /// <summary>Inserts a new PC station if it does not already exist. Idempotent.</summary>
    void Insert(string pcName);
}
