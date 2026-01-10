namespace PEMetrics.DataApi.Models;

/// <summary>
/// Represents a PC workstation (floor.PCStation).
/// </summary>
public sealed record PCStation
{
    public required string PcName { get; init; }
}
