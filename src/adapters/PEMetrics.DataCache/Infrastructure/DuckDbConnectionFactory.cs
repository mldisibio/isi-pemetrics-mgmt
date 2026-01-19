using System.Data.Common;
using DuckDB.NET.Data;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataCache.Configuration;

namespace PEMetrics.DataCache.Infrastructure;

/// <summary>Creates and manages the single DuckDB connection for the cache database.</summary>
/// <remarks>
/// DuckDB is an in-process database, so we maintain a single long-lived connection
/// rather than opening/closing connections for each query. The nanodbc extension
/// is installed and loaded once when the connection is first opened.
/// </remarks>
public sealed class DuckDbConnectionFactory : ForCreatingDuckDbConnections, IDisposable
{
    readonly CacheConfiguration _configuration;
    readonly string _connectionString;
    readonly object _lock = new();
    readonly string _resolvedPath;
    DuckDBConnection? _connection;
    bool _nanodbcLoaded;
    bool _disposed;

    public DuckDbConnectionFactory(CacheConfiguration configuration, CachePathResolver pathResolver)
    {
        ArgumentNullException.ThrowIfNull(pathResolver);
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _resolvedPath = pathResolver.ResolvePathAndEnsureDirectory(_configuration.CachePath);
        _connectionString = $"Data Source={_resolvedPath}";
    }

    public async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DuckDbConnectionFactory));

        // Double-checked locking for thread-safe lazy initialization
        if (_connection == null)
        {
            lock (_lock)
            {
                _connection ??= new DuckDBConnection(_connectionString);
            }
        }

        if (_connection.State != System.Data.ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await InstallAndLoadNanodbcAsync(cancellationToken).ConfigureAwait(false);
        }

        // Install and load nanodbc extension once
        if (!_nanodbcLoaded)
        {
            await InstallAndLoadNanodbcAsync(cancellationToken).ConfigureAwait(false);
        }

        // Return a wrapper that prevents disposal of the shared connection
        return _connection;
    }

    async Task InstallAndLoadNanodbcAsync(CancellationToken cancellationToken)
    {
        if (_connection == null)
            return;

        await using var installCmd = _connection.CreateCommand();
        installCmd.CommandText = "INSTALL nanodbc FROM community;";
        await installCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        await using var loadCmd = _connection.CreateCommand();
        loadCmd.CommandText = "LOAD nanodbc;";
        await loadCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _nanodbcLoaded = true;
    }

    string CompactDatabase()
    {
        using var dbConnection = new DuckDBConnection($"Data Source={DuckDBConnectionStringBuilder.InMemoryDataSource}");
        dbConnection.Open();
        using var cmd = dbConnection.CreateCommand();
        cmd.CommandText = $@"
ATTACH '{_resolvedPath}' AS existingDb;
ATTACH '{_resolvedPath}.tmp' AS tempDb;
COPY FROM DATABASE existingDb TO tempDb;
";
        cmd.ExecuteNonQuery();
        dbConnection.Close();
        // Replace original database file with compacted version
        File.Delete(_resolvedPath);
        File.Move($"{_resolvedPath}.tmp", _resolvedPath);
        return _resolvedPath;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _connection?.Dispose();
        _connection = null;

        if (_configuration.CompactOnExit && File.Exists(_resolvedPath))
        {
            try
            {
                CompactDatabase();
            }
            catch
            {
                // Ignore cleanup errors on exit
            }
        }
    }
}
