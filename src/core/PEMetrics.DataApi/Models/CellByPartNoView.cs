namespace PEMetrics.DataApi.Models;

/// <summary>Expanded view of part number to cell mappings from mgmt.vw_CellByPartNo.</summary>
public sealed record CellByPartNoView
{
    public required string PartNo { get; init; }
    public string? Family { get; init; }
    public string? Subfamily { get; init; }
    public string? Description { get; init; }
    public required int CellId { get; init; }
    public string? CellName { get; init; }
}
