using System.Collections.Immutable;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing software test to cell mappings.</summary>
public interface ForMappingSwTestsToCells
{
    /// <summary>Retrieves all software test to cell mappings from mgmt.vw_CellBySwTest.</summary>
    ImmutableList<CellBySwTest> GetAll();

    /// <summary>Retrieves all cell mappings for a specific software test.</summary>
    ImmutableList<CellBySwTest> GetBySwTestMapId(int swTestMapId);

    /// <summary>Replaces all cell mappings for a software test. Atomic operation.</summary>
    void SetMappings(int swTestMapId, IEnumerable<int> cellIds);

    /// <summary>Adds a single cell mapping for a software test. Idempotent.</summary>
    void AddMapping(int swTestMapId, int cellId);

    /// <summary>Removes a single cell mapping for a software test. Idempotent.</summary>
    void DeleteMapping(int swTestMapId, int cellId);
}
