using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>
/// Port for managing software test to cell mappings.
/// </summary>
public interface ICellBySwTestRepository
{
    /// <summary>
    /// Retrieves all cell mappings for a specific software test.
    /// </summary>
    IEnumerable<CellBySwTest> GetBySwTestMapId(int swTestMapId);

    /// <summary>
    /// Replaces all cell mappings for a software test.
    /// Atomic operation: deletes existing, inserts new.
    /// </summary>
    void SetMappings(int swTestMapId, IEnumerable<int> cellIds);

    /// <summary>
    /// Adds a single cell mapping for a software test.
    /// Idempotent: silently succeeds if mapping already exists.
    /// </summary>
    void AddMapping(int swTestMapId, int cellId);

    /// <summary>
    /// Removes a single cell mapping for a software test.
    /// Idempotent: silently succeeds if mapping does not exist.
    /// </summary>
    void DeleteMapping(int swTestMapId, int cellId);
}
