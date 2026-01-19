namespace PEMetrics.DataCache.Configuration;

/// <summary>Configuration for the DuckDB cache layer. Bound from appsettings.json CacheConfiguration section.</summary>
public sealed class CacheConfiguration
{
    /// <summary>Type of cache database. Currently only "DuckDb" is supported.</summary>
    public string CacheDbType { get; set; } = "DuckDb";

    /// <summary>Path to the DuckDB cache file. Supports MyDocuments prefix, absolute, or relative paths.</summary>
    public string CachePath { get; set; } = "MyDocuments\\PEDimMgmnt\\PE_Metrics_Cache.duckdb";

    /// <summary>If true, delete the cache file on application exit.</summary>
    public bool DeleteOnExit { get; set; }

    /// <summary>If true, compact the cache file on application exit.</summary>
    public bool CompactOnExit { get; set; }

    /// <summary>Path to DuckDB initialization SQL script. Supports MyDocuments prefix, absolute, or relative paths.</summary>
    public string InitSqlPath { get; set; } = "MyDocuments\\PEDimMgmnt\\duckdb_init.sql";

    /// <summary>Maximum number of tables to populate in parallel during startup.</summary>
    public int MaxParallelPopulation { get; set; } = 4;
}
