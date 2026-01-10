using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>
/// Port for managing production cells.
/// </summary>
public interface ICellRepository
{
    /// <summary>
    /// Retrieves all cells from mgmt.vw_Cell.
    /// </summary>
    IEnumerable<Cell> GetAll();

    /// <summary>
    /// Retrieves a single cell by ID.
    /// </summary>
    /// <returns>The cell, or null if not found.</returns>
    Cell? GetById(int cellId);

    /// <summary>
    /// Inserts a new cell.
    /// </summary>
    /// <returns>The new CellId.</returns>
    int Insert(Cell cell);

    /// <summary>
    /// Updates an existing cell.
    /// </summary>
    void Update(Cell cell);
}
