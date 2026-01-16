using System.Data.Common;
using DuckDB.NET.Data;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataCache.Configuration;

namespace PEMetrics.DataCache.Infrastructure;

/// <summary>Creates connections to the DuckDB cache database.</summary>
public sealed class DuckDbConnectionFactory : ForCreatingDuckDbConnections
{
    readonly string _connectionString;

    public DuckDbConnectionFactory(CacheConfiguration configuration, CachePathResolver pathResolver)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(pathResolver);

        var resolvedPath = pathResolver.ResolvePathAndEnsureDirectory(configuration.CachePath);
        _connectionString = $"Data Source={resolvedPath}";
    }

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new DuckDBConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
