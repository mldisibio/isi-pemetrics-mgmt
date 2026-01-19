using System.Data.Common;
using DuckDB.NET.Data;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataCache.Infrastructure;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Creates DuckDB connections for testing. Uses a temp file to share data across connections.</summary>
public sealed class TestDuckDbConnectionFactory : ForCreatingDuckDbConnections, IDisposable
{
    readonly string _dbPath;
    readonly string _connectionString;
    readonly object _lock = new();
    DuckDBConnection? _connection;
    bool _nanodbcInstalled;
    bool _disposed;

    public TestDuckDbConnectionFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"pe_metrics_test_{Guid.NewGuid():N}.duckdb");
        _connectionString = $"Data Source={_dbPath}";
    }

    public string DatabasePath => _dbPath;

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
        if (!_nanodbcInstalled)
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

        _nanodbcInstalled = true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _connection?.Dispose();
        _connection = null;

        // Clean up temp database files
        try
        {
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);

            var walPath = _dbPath + ".wal";
            if (File.Exists(walPath))
                File.Delete(walPath);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
