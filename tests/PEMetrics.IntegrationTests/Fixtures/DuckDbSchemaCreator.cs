using System.Data.Common;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Creates DuckDB schema tables for testing using production init script.</summary>
public static class DuckDbSchemaCreator
{
    /// <summary>Creates all cache tables in DuckDB using the production duckdb_init.sql script.</summary>
    public static void CreateTables(DbConnection connection)
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "DuckDbScripts", "duckdb_init.sql");
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"DuckDB init script not found at: {scriptPath}");

        var sql = File.ReadAllText(scriptPath);

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    /// <summary>Installs and loads the nanodbc extension.</summary>
    public static void InstallNanodbc(DbConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "INSTALL nanodbc FROM community;";
        command.ExecuteNonQuery();

        command.CommandText = "LOAD nanodbc;";
        command.ExecuteNonQuery();
    }
}
