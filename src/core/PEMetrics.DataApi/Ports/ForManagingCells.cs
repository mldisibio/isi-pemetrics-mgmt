using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing production cells.</summary>
public interface ForManagingCells
{
    /// <summary>Inserts a new cell.</summary>
    /// <returns>The new CellId, or -1 on error.</returns>
    int Insert(Cell cell);

    /// <summary>Updates an existing cell.</summary>
    /// <returns>True if successful, false on error.</returns>
    bool Update(Cell cell);
}
