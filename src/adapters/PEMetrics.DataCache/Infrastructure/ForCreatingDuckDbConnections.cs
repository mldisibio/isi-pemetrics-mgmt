using DuckDB.NET.Data;

namespace PEMetrics.DataCache.Infrastructure;

/// <summary>Port for creating DuckDB connections.</summary>
public interface ForCreatingDuckDbConnections
{
    /// <summary>Creates and opens a connection to the DuckDB cache database.</summary>
    DuckDBConnection OpenConnection();
}
