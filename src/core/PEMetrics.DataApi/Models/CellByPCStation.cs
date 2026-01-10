namespace PEMetrics.DataApi.Models;

/// <summary>
/// Represents a PC-to-Cell mapping (floor.CellByPCStation).
/// Includes denormalized Cell name from joined data.
/// </summary>
public sealed record CellByPCStation
{
    public required int StationMapId { get; init; }
    public required int CellId { get; init; }
    public required string PcName { get; init; }
    public string? PcPurpose { get; init; }
    public required DateOnly ActiveFrom { get; init; }
    public DateOnly? ActiveTo { get; init; }
    public string? ExtendedName { get; init; }

    /// <summary>
    /// Denormalized from floor.Cell join.
    /// </summary>
    public string? CellName { get; init; }

    /// <summary>
    /// Computed: true if ActiveTo is null or >= today.
    /// </summary>
    public bool IsActive { get; init; }
}
