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
    public async Task InitializeAsync(string sqlServerOdbcConnectionString, CancellationToken cancellationToken = default)
    {
        OdbcConnectionString = sqlServerOdbcConnectionString;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        // Install and load nanodbc
        await DuckDbSchemaCreator.InstallNanodbcAsync(connection, cancellationToken);

        // Create cache tables
        await DuckDbSchemaCreator.CreateTablesAsync(connection, cancellationToken);
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
