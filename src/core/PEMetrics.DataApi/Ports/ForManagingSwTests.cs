using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing software test mappings.</summary>
public interface ForManagingSwTests
{
    /// <summary>Inserts a new software test.</summary>
    /// <returns>The new SwTestMapId, or -1 on error.</returns>
    Task<int> InsertAsync(SwTestMap test, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing software test.</summary>
    /// <returns>True if successful, false on error.</returns>
    Task<bool> UpdateAsync(SwTestMap test, CancellationToken cancellationToken = default);
}
