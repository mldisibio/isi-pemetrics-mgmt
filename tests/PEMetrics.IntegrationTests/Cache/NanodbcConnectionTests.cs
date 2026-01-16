using DuckDB.NET.Data;
using PEMetrics.IntegrationTests.Fixtures;

namespace PEMetrics.IntegrationTests.Cache;

/// <summary>Tests verifying nanodbc extension can connect to SQL Server from DuckDB.</summary>
[Collection("SqlServerCollection")]
public sealed class NanodbcConnectionTests : IDisposable
{
    readonly SqlServerContainerFixture _sqlFixture;
    readonly TestDuckDbConnectionFactory _duckDbFactory;
    readonly string _odbcConnectionString;

    public NanodbcConnectionTests(SqlServerContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
        _duckDbFactory = new TestDuckDbConnectionFactory();
        _odbcConnectionString = OdbcConnectionStringBuilder.ToOdbcConnectionString(sqlFixture.ConnectionString);
    }

    public void Dispose() => _duckDbFactory.Dispose();

    [Fact]
    public void NanodbcExtension_InstallsAndLoads()
    {
        using var connection = _duckDbFactory.OpenConnection();

        DuckDbSchemaCreator.InstallNanodbc(connection);

        // Verify extension is loaded by checking duckdb_extensions()
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT extension_name FROM duckdb_extensions() WHERE loaded = true AND extension_name = 'nanodbc'";
        using var reader = command.ExecuteReader();

        Assert.True(reader.Read());
        Assert.Equal("nanodbc", reader.GetString(0));
    }

    [Fact]
    public void OdbcScan_CanQuerySqlServerCell()
    {
        using var connection = _duckDbFactory.OpenConnection();
        DuckDbSchemaCreator.InstallNanodbc(connection);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT COUNT(*) FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_Cell',
                read_only=true
            )";

        var count = Convert.ToInt64(command.ExecuteScalar());

        Assert.True(count > 0, "Expected at least one cell from SQL Server");
    }

    [Fact]
    public void OdbcScan_CanQuerySqlServerPCStation()
    {
        using var connection = _duckDbFactory.OpenConnection();
        DuckDbSchemaCreator.InstallNanodbc(connection);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT COUNT(*) FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_PCStation',
                read_only=true
            )";

        var count = Convert.ToInt64(command.ExecuteScalar());

        Assert.True(count > 0, "Expected at least one PC station from SQL Server");
    }

    [Fact]
    public void OdbcScan_CanQuerySqlServerSwTestMap()
    {
        using var connection = _duckDbFactory.OpenConnection();
        DuckDbSchemaCreator.InstallNanodbc(connection);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT COUNT(*) FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_SwTestMap',
                read_only=true
            )";

        var count = Convert.ToInt64(command.ExecuteScalar());

        Assert.True(count > 0, "Expected at least one software test from SQL Server");
    }

    [Fact]
    public void OdbcScan_CanQuerySqlServerTLA()
    {
        using var connection = _duckDbFactory.OpenConnection();
        DuckDbSchemaCreator.InstallNanodbc(connection);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT COUNT(*) FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_TLA',
                read_only=true
            )";

        var count = Convert.ToInt64(command.ExecuteScalar());

        Assert.True(count > 0, "Expected at least one TLA from SQL Server");
    }

    [Fact]
    public void OdbcScan_CanInsertIntoLocalTable()
    {
        using var connection = _duckDbFactory.OpenConnection();
        DuckDbSchemaCreator.InstallNanodbc(connection);
        DuckDbSchemaCreator.CreateTables(connection);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            INSERT INTO Cell
            SELECT * FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_Cell',
                read_only=true
            )";
        command.ExecuteNonQuery();

        // Verify data was inserted
        command.CommandText = "SELECT COUNT(*) FROM Cell";
        var count = Convert.ToInt64(command.ExecuteScalar());

        Assert.True(count > 0, "Expected cells to be populated in DuckDB table");
    }

    static string EscapeSql(string value) => value.Replace("'", "''");
}
