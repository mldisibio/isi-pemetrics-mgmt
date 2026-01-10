using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>
/// Port for managing part number to cell mappings.
/// </summary>
public interface ICellByPartNoRepository
{
    /// <summary>
    /// Retrieves all cell mappings for a specific part number.
    /// </summary>
    IEnumerable<CellByPartNo> GetByPartNo(string partNo);

    /// <summary>
    /// Replaces all cell mappings for a part number.
    /// Atomic operation: deletes existing, inserts new.
    /// </summary>
    void SetMappings(string partNo, IEnumerable<int> cellIds);

    /// <summary>
    /// Adds a single cell mapping for a part number.
    /// Idempotent: silently succeeds if mapping already exists.
    /// </summary>
    void AddMapping(string partNo, int cellId);

    /// <summary>
    /// Removes a single cell mapping for a part number.
    /// Idempotent: silently succeeds if mapping does not exist.
    /// </summary>
    void DeleteMapping(string partNo, int cellId);
}
