namespace PEMetrics.DataApi.Models;

/// <summary>
/// Represents a production cell (floor.Cell).
/// </summary>
public sealed record Cell
{
    public required int CellId { get; init; }
    public required string CellName { get; init; }
    public required string DisplayName { get; init; }
    public required DateOnly ActiveFrom { get; init; }
    public DateOnly? ActiveTo { get; init; }
    public string? Description { get; init; }
    public string? AlternativeNames { get; init; }

    /// <summary>
    /// Computed: true if ActiveTo is null or >= today.
    /// </summary>
    public bool IsActive { get; init; }
}
