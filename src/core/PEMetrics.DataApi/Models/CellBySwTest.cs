namespace PEMetrics.DataApi.Models;

/// <summary>
/// Represents a software test to cell mapping (floor.CellBySwTest).
/// Includes denormalized Cell name from joined data.
/// </summary>
public sealed record CellBySwTest
{
    public required int SwTestMapId { get; init; }
    public required int CellId { get; init; }

    /// <summary>
    /// Denormalized from floor.Cell join.
    /// </summary>
    public string? CellName { get; init; }
}
