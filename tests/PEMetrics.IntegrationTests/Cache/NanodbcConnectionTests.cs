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
    public async Task NanodbcExtension_InstallsAndLoads()
    {
        var connection = await _duckDbFactory.GetOpenConnectionAsync();

        await DuckDbSchemaCreator.InstallNanodbcAsync(connection);

        // Verify extension is loaded by checking duckdb_extensions()
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT extension_name FROM duckdb_extensions() WHERE loaded = true AND extension_name = 'nanodbc'";
        await using var reader = await command.ExecuteReaderAsync();

        Assert.True(await reader.ReadAsync());
        Assert.Equal("nanodbc", reader.GetString(0));
    }

    [Fact]
    public async Task OdbcScan_CanQuerySqlServerCell()
    {
        var connection = await _duckDbFactory.GetOpenConnectionAsync();
        await DuckDbSchemaCreator.InstallNanodbcAsync(connection);

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT COUNT(*) FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_Cell',
                read_only=true
            )";

        var count = Convert.ToInt64(await command.ExecuteScalarAsync());

        Assert.True(count > 0, "Expected at least one cell from SQL Server");
    }

    [Fact]
    public async Task OdbcScan_CanQuerySqlServerPCStation()
    {
        var connection = await _duckDbFactory.GetOpenConnectionAsync();
        await DuckDbSchemaCreator.InstallNanodbcAsync(connection);

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT COUNT(*) FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_PCStation',
                read_only=true
            )";

        var count = Convert.ToInt64(await command.ExecuteScalarAsync());

        Assert.True(count > 0, "Expected at least one PC station from SQL Server");
    }

    [Fact]
    public async Task OdbcScan_CanQuerySqlServerSwTestMap()
    {
        var connection = await _duckDbFactory.GetOpenConnectionAsync();
        await DuckDbSchemaCreator.InstallNanodbcAsync(connection);

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT COUNT(*) FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_SwTestMap',
                read_only=true
            )";

        var count = Convert.ToInt64(await command.ExecuteScalarAsync());

        Assert.True(count > 0, "Expected at least one software test from SQL Server");
    }

    [Fact]
    public async Task OdbcScan_CanQuerySqlServerTLA()
    {
        var connection = await _duckDbFactory.GetOpenConnectionAsync();
        await DuckDbSchemaCreator.InstallNanodbcAsync(connection);

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT COUNT(*) FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_TLA',
                read_only=true
            )";

        var count = Convert.ToInt64(await command.ExecuteScalarAsync());

        Assert.True(count > 0, "Expected at least one TLA from SQL Server");
    }

    [Fact]
    public async Task OdbcScan_CanInsertIntoLocalTable()
    {
        var connection = await _duckDbFactory.GetOpenConnectionAsync();
        await DuckDbSchemaCreator.InstallNanodbcAsync(connection);
        await DuckDbSchemaCreator.CreateTablesAsync(connection);

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            INSERT INTO Cell
            SELECT * FROM odbc_scan(
                connection='{EscapeSql(_odbcConnectionString)}',
                schema_name='mgmt',
                table_name='vw_Cell',
                read_only=true
            )";
        await command.ExecuteNonQueryAsync();

        // Verify data was inserted
        command.CommandText = "SELECT COUNT(*) FROM Cell";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync());

        Assert.True(count > 0, "Expected cells to be populated in DuckDB table");
    }

    static string EscapeSql(string value) => value.Replace("'", "''");
}
