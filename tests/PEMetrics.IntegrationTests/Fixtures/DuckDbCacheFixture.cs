using DuckDB.NET.Data;
using PEMetrics.DataApi.Infrastructure;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Fixture for DuckDB cache tests. Sets up DuckDB with nanodbc and creates tables.</summary>
public sealed class DuckDbCacheFixture : IDisposable
{
    readonly TestDuckDbConnectionFactory _connectionFactory;
    bool _disposed;

    public DuckDbCacheFixture()
    {
        _connectionFactory = new TestDuckDbConnectionFactory();
    }

    /// <summary>Gets the DuckDB connection factory.</summary>
    public ForCreatingDuckDbConnections ConnectionFactory => _connectionFactory;

    /// <summary>Initializes DuckDB with nanodbc and creates tables.</summary>
    public void Initialize(string sqlServerOdbcConnectionString)
    {
        OdbcConnectionString = sqlServerOdbcConnectionString;

        using var connection = _connectionFactory.OpenConnection();

        // Install and load nanodbc
        DuckDbSchemaCreator.InstallNanodbc(connection);

        // Create cache tables
        DuckDbSchemaCreator.CreateTables(connection);
    }

    /// <summary>Gets the ODBC connection string for connecting to SQL Server from DuckDB.</summary>
    public string OdbcConnectionString { get; private set; } = string.Empty;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _connectionFactory.Dispose();
    }
}
