using System.Data.Common;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Shared SQL Server container fixture for integration tests.</summary>
public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    MsSqlContainer? _container;

    public string ConnectionString => _container?.GetConnectionString()
        ?? throw new InvalidOperationException("Container not initialized");

    public async Task InitializeAsync()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .Build();

        await _container.StartAsync();
        await InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }

    async Task InitializeDatabaseAsync()
    {
        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "SqlScripts");

        await ExecuteScriptAsync(Path.Combine(scriptsPath, "01_CreateSchemas.sql"));
        await ExecuteScriptAsync(Path.Combine(scriptsPath, "02_CreateBaseTables.sql"));
        await ExecuteScriptAsync(Path.Combine(scriptsPath, "03_CreateMgmtObjects.sql"));
        await ExecuteScriptAsync(Path.Combine(scriptsPath, "04_SeedData.sql"));
    }

    async Task ExecuteScriptAsync(string scriptPath)
    {
        var script = await File.ReadAllTextAsync(scriptPath);

        // Split on GO statements (SQL Server batch separator)
        var batches = script.Split(
            ["GO", "go", "Go"],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        foreach (var batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch))
                continue;

            await using var command = new SqlCommand(batch, connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    /// <summary>Creates a new open connection to the test database.</summary>
    public DbConnection CreateConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }
}
