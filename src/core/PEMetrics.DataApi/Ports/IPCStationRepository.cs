using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>
/// Port for managing PC workstations.
/// </summary>
public interface IPCStationRepository
{
    /// <summary>
    /// Retrieves all PC stations from mgmt.vw_PCStation.
    /// </summary>
    IEnumerable<PCStation> GetAll();

    /// <summary>
    /// Searches PC stations by name prefix (for autocomplete).
    /// </summary>
    /// <param name="prefix">The prefix to search for.</param>
    /// <returns>Matching PC stations sorted alphabetically.</returns>
    IEnumerable<PCStation> Search(string prefix);

    /// <summary>
    /// Inserts a new PC station if it does not already exist.
    /// Idempotent: silently succeeds if name already exists.
    /// </summary>
    void Insert(string pcName);
}
