using System.Collections.Immutable;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing part number to cell mappings.</summary>
public interface ForMappingPartNumberToCells
{
    /// <summary>Retrieves all cell mappings for a specific part number.</summary>
    ImmutableList<CellByPartNo> GetByPartNo(string partNo);

    /// <summary>Replaces all cell mappings for a part number. Atomic operation.</summary>
    void SetMappings(string partNo, IEnumerable<int> cellIds);

    /// <summary>Adds a single cell mapping for a part number. Idempotent.</summary>
    void AddMapping(string partNo, int cellId);

    /// <summary>Removes a single cell mapping for a part number. Idempotent.</summary>
    void DeleteMapping(string partNo, int cellId);
}
