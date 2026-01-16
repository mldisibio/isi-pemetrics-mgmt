namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing part number to cell mappings.</summary>
public interface ForMappingPartNumberToCells
{
    /// <summary>Replaces all cell mappings for a part number. Atomic operation.</summary>
    /// <returns>True if successful, false on error.</returns>
    Task<bool> SetMappingsAsync(string partNo, IEnumerable<int> cellIds, CancellationToken cancellationToken = default);

    /// <summary>Adds a single cell mapping for a part number. Idempotent.</summary>
    /// <returns>True if successful, false on error.</returns>
    Task<bool> AddMappingAsync(string partNo, int cellId, CancellationToken cancellationToken = default);

    /// <summary>Removes a single cell mapping for a part number. Idempotent.</summary>
    /// <returns>True if successful, false on error.</returns>
    Task<bool> DeleteMappingAsync(string partNo, int cellId, CancellationToken cancellationToken = default);
}
