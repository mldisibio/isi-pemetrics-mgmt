using System.Data.Common;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Creates DuckDB schema tables for testing using production init script.</summary>
public static class DuckDbSchemaCreator
{
    /// <summary>Creates all cache tables in DuckDB using the production duckdb_init.sql script.</summary>
    public static async Task CreateTablesAsync(DbConnection connection, CancellationToken cancellationToken = default)
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "DuckDbScripts", "duckdb_init.sql");
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"DuckDB init script not found at: {scriptPath}");

        var sql = await File.ReadAllTextAsync(scriptPath, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>Installs and loads the nanodbc extension.</summary>
    public static async Task InstallNanodbcAsync(DbConnection connection, CancellationToken cancellationToken = default)
    {
        await using var command = connection.CreateCommand();

        command.CommandText = "INSTALL nanodbc FROM community;";
        await command.ExecuteNonQueryAsync(cancellationToken);

        command.CommandText = "LOAD nanodbc;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
