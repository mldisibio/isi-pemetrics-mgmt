using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing part numbers (TLAs).</summary>
public interface ForManagingPartNumbers
{
    /// <summary>Inserts a new TLA.</summary>
    void Insert(TLA tla);

    /// <summary>Updates an existing TLA.</summary>
    void Update(TLA tla);

    /// <summary>Deletes a TLA if not in use. Idempotent if not found. Throws if in use.</summary>
    void Delete(string partNo);
}
