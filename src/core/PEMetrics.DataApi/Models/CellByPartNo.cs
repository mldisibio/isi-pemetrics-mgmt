namespace PEMetrics.DataApi.Models;

/// <summary>
/// Represents a part number to cell mapping (floor.CellByPartNo).
/// Includes denormalized Cell name from joined data.
/// </summary>
public sealed record CellByPartNo
{
    public required string PartNo { get; init; }
    public required int CellId { get; init; }

    /// <summary>
    /// Denormalized from floor.Cell join.
    /// </summary>
    public string? CellName { get; init; }
}
