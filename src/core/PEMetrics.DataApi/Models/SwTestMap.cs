namespace PEMetrics.DataApi.Models;

/// <summary>
/// Represents a software test mapping (sw.SwTestMap).
/// Maps ConfiguredTestId + TestName to a ReportKey for metrics.
/// </summary>
public sealed record SwTestMap
{
    public required int SwTestMapId { get; init; }
    public string? ConfiguredTestId { get; init; }
    public string? TestApplication { get; init; }
    public string? TestName { get; init; }
    public string? ReportKey { get; init; }
    public string? TestDirectory { get; init; }
    public string? RelativePath { get; init; }
    public DateOnly? LastRun { get; init; }
    public string? Notes { get; init; }

    /// <summary>
    /// Computed: true if LastRun is null or within past 3 months.
    /// </summary>
    public bool IsActive { get; init; }
}
