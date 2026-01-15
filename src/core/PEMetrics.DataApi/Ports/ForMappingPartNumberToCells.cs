namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing part number to cell mappings.</summary>
public interface ForMappingPartNumberToCells
{
    /// <summary>Replaces all cell mappings for a part number. Atomic operation.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool SetMappings(string partNo, IEnumerable<int> cellIds);

    /// <summary>Adds a single cell mapping for a part number. Idempotent.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool AddMapping(string partNo, int cellId);

    /// <summary>Removes a single cell mapping for a part number. Idempotent.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool DeleteMapping(string partNo, int cellId);
}
