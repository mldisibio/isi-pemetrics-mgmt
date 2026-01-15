namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing software test to cell mappings.</summary>
public interface ForMappingSwTestsToCells
{
    /// <summary>Replaces all cell mappings for a software test. Atomic operation.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool SetMappings(int swTestMapId, IEnumerable<int> cellIds);

    /// <summary>Adds a single cell mapping for a software test. Idempotent.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool AddMapping(int swTestMapId, int cellId);

    /// <summary>Removes a single cell mapping for a software test. Idempotent.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool DeleteMapping(int swTestMapId, int cellId);
}
