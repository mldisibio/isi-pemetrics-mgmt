using System.Data.Common;

namespace PEMetrics.DataApi.Infrastructure;

/// <summary>Port for creating DuckDB connections.</summary>
public interface ForCreatingDuckDbConnections
{
    /// <summary>Creates and opens a connection to the DuckDB cache database.</summary>
    Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default);
}
