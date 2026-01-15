namespace PEMetrics.DataCache.Services;

/// <summary>Represents a request to refresh one or more cache tables.</summary>
public sealed record RefreshRequest
{
    /// <summary>Tables that need to be refreshed.</summary>
    public required IReadOnlyList<string> Tables { get; init; }

    /// <summary>Source of the refresh request for logging/debugging.</summary>
    public string? Source { get; init; }

    /// <summary>Creates a refresh request for a single table.</summary>
    public static RefreshRequest ForTable(string tableName, string? source = null) =>
        new() { Tables = [tableName], Source = source };

    /// <summary>Creates a refresh request for multiple tables.</summary>
    public static RefreshRequest ForTables(IEnumerable<string> tableNames, string? source = null) =>
        new() { Tables = tableNames.ToList(), Source = source };
}
