namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing software test to cell mappings.</summary>
public interface ForMappingSwTestsToCells
{
    /// <summary>Replaces all cell mappings for a software test. Atomic operation.</summary>
    /// <returns>True if successful, false on error.</returns>
    Task<bool> SetMappingsAsync(int swTestMapId, IEnumerable<int> cellIds, CancellationToken cancellationToken = default);

    /// <summary>Adds a single cell mapping for a software test. Idempotent.</summary>
    /// <returns>True if successful, false on error.</returns>
    Task<bool> AddMappingAsync(int swTestMapId, int cellId, CancellationToken cancellationToken = default);

    /// <summary>Removes a single cell mapping for a software test. Idempotent.</summary>
    /// <returns>True if successful, false on error.</returns>
    Task<bool> DeleteMappingAsync(int swTestMapId, int cellId, CancellationToken cancellationToken = default);
}
