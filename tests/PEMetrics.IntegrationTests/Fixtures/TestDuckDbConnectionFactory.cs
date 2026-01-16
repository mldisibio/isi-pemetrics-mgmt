using System.Data.Common;
using DuckDB.NET.Data;
using PEMetrics.DataApi.Infrastructure;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Creates DuckDB connections for testing. Uses a temp file to share data across connections.</summary>
public sealed class TestDuckDbConnectionFactory : ForCreatingDuckDbConnections, IDisposable
{
    readonly string _dbPath;
    readonly string _connectionString;
    bool _nanodbcInstalled;
    bool _disposed;

    public TestDuckDbConnectionFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"pe_metrics_test_{Guid.NewGuid():N}.duckdb");
        _connectionString = $"Data Source={_dbPath}";
    }

    public string DatabasePath => _dbPath;

    /// <summary>Installs nanodbc extension (call once before using odbc_scan).</summary>
    public async Task InstallNanodbcAsync(CancellationToken cancellationToken = default)
    {
        if (_nanodbcInstalled)
            return;

        await using var connection = new DuckDBConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSTALL nanodbc FROM community;";
        await command.ExecuteNonQueryAsync(cancellationToken);
        _nanodbcInstalled = true;
    }

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new DuckDBConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Auto-load nanodbc if it was installed
        if (_nanodbcInstalled)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "LOAD nanodbc;";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return connection;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

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
