namespace PEMetrics.DataApi.Models;

/// <summary>Expanded view of software test to cell mappings from mgmt.vw_CellBySwTest.</summary>
public sealed record CellBySwTestView
{
    public required int SwTestMapId { get; init; }
    public string? ConfiguredTestId { get; init; }
    public string? TestApplication { get; init; }
    public string? ReportKey { get; init; }
    public DateOnly? LastRun { get; init; }
    public bool IsActive { get; init; }
    public required int CellId { get; init; }
    public string? CellName { get; init; }
}
