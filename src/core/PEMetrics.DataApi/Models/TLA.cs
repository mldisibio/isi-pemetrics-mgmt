namespace PEMetrics.DataApi.Models;

/// <summary>
/// Represents a top-level assembly / part number (product.TLA).
/// </summary>
public sealed record TLA
{
    public required string PartNo { get; init; }
    public string? Family { get; init; }
    public string? Subfamily { get; init; }
    public string? ServiceGroup { get; init; }
    public string? FormalDescription { get; init; }
    public string? Description { get; init; }

    /// <summary>
    /// Computed: true if PartNo exists in activity.ProductionTest.
    /// Used to determine if hard delete is allowed.
    /// </summary>
    public bool IsUsed { get; init; }
}
