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
    public void InstallNanodbc()
    {
        if (_nanodbcInstalled)
            return;

        using var connection = new DuckDBConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSTALL nanodbc FROM community;";
        command.ExecuteNonQuery();
        _nanodbcInstalled = true;
    }

    public DbConnection OpenConnection()
    {
        var connection = new DuckDBConnection(_connectionString);
        connection.Open();

        // Auto-load nanodbc if it was installed
        if (_nanodbcInstalled)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "LOAD nanodbc;";
            command.ExecuteNonQuery();
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
