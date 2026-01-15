using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing part numbers (TLAs).</summary>
public interface ForManagingPartNumbers
{
    /// <summary>Inserts a new TLA.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool Insert(TLA tla);

    /// <summary>Updates an existing TLA.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool Update(TLA tla);

    /// <summary>Deletes a TLA if not in use. Idempotent if not found.</summary>
    /// <returns>True if successful, false on error (including if TLA is in use).</returns>
    bool Delete(string partNo);
}
