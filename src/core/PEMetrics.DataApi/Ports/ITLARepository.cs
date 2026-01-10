using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>
/// Port for managing part numbers (TLAs).
/// </summary>
public interface ITLARepository
{
    /// <summary>
    /// Retrieves all TLAs from mgmt.vw_TLA.
    /// </summary>
    IEnumerable<TLA> GetAll();

    /// <summary>
    /// Retrieves a single TLA by part number.
    /// </summary>
    /// <returns>The TLA, or null if not found.</returns>
    TLA? GetByPartNo(string partNo);

    /// <summary>
    /// Inserts a new TLA.
    /// </summary>
    void Insert(TLA tla);

    /// <summary>
    /// Updates an existing TLA.
    /// </summary>
    void Update(TLA tla);

    /// <summary>
    /// Deletes a TLA if it is not in use.
    /// Idempotent: silently succeeds if part number does not exist.
    /// Throws if TLA is referenced in production tests or cell mappings.
    /// </summary>
    void Delete(string partNo);
}
