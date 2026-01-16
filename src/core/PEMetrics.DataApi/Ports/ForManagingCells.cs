using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing production cells.</summary>
public interface ForManagingCells
{
    /// <summary>Inserts a new cell.</summary>
    /// <returns>The new CellId, or -1 on error.</returns>
    Task<int> InsertAsync(Cell cell, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing cell.</summary>
    /// <returns>True if successful, false on error.</returns>
    Task<bool> UpdateAsync(Cell cell, CancellationToken cancellationToken = default);
}
