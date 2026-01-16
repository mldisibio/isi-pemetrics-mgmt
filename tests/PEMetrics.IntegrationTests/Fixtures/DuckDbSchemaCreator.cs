using System.Data.Common;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Creates DuckDB schema tables for testing.</summary>
public static class DuckDbSchemaCreator
{
    /// <summary>Creates all cache tables in DuckDB.</summary>
    public static void CreateTables(DbConnection connection)
    {
        using var command = connection.CreateCommand();

        // Tables must match SQL Server view column order exactly for SELECT * inserts
        // Use INTEGER for IsActive/IsUsed to match SQL Server CASE output (0/1)
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Cell (
                CellId INTEGER NOT NULL,
                CellName VARCHAR NOT NULL,
                DisplayName VARCHAR NOT NULL,
                ActiveFrom DATE NOT NULL,
                ActiveTo DATE,
                Description VARCHAR,
                AlternativeNames VARCHAR,
                IsActive INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS PCStation (
                PcName VARCHAR NOT NULL
            );

            -- vw_CellByPCStation column order: StationMapId, CellId, CellName, PcName, PcPurpose, ActiveFrom, ActiveTo, ExtendedName, IsActive
            CREATE TABLE IF NOT EXISTS CellByPCStation (
                StationMapId INTEGER NOT NULL,
                CellId INTEGER NOT NULL,
                CellName VARCHAR,
                PcName VARCHAR NOT NULL,
                PcPurpose VARCHAR,
                ActiveFrom DATE NOT NULL,
                ActiveTo DATE,
                ExtendedName VARCHAR,
                IsActive INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS SwTestMap (
                SwTestMapId INTEGER NOT NULL,
                ConfiguredTestId VARCHAR,
                TestApplication VARCHAR,
                TestName VARCHAR,
                ReportKey VARCHAR,
                TestDirectory VARCHAR,
                RelativePath VARCHAR,
                LastRun DATE,
                Notes VARCHAR,
                IsActive INTEGER NOT NULL
            );

            -- vw_CellBySwTest column order: SwTestMapId, ConfiguredTestId, TestApplication, ReportKey, LastRun, IsActive, CellId, CellName
            -- Both CellBySwTest and CellBySwTestView are populated from this view
            CREATE TABLE IF NOT EXISTS CellBySwTest (
                SwTestMapId INTEGER NOT NULL,
                ConfiguredTestId VARCHAR,
                TestApplication VARCHAR,
                ReportKey VARCHAR,
                LastRun DATE,
                IsActive INTEGER NOT NULL,
                CellId INTEGER NOT NULL,
                CellName VARCHAR
            );

            CREATE TABLE IF NOT EXISTS CellBySwTestView (
                SwTestMapId INTEGER NOT NULL,
                ConfiguredTestId VARCHAR,
                TestApplication VARCHAR,
                ReportKey VARCHAR,
                LastRun DATE,
                IsActive INTEGER NOT NULL,
                CellId INTEGER NOT NULL,
                CellName VARCHAR
            );

            CREATE TABLE IF NOT EXISTS TLA (
                PartNo VARCHAR NOT NULL,
                Family VARCHAR,
                Subfamily VARCHAR,
                ServiceGroup VARCHAR,
                FormalDescription VARCHAR,
                Description VARCHAR,
                IsUsed INTEGER NOT NULL
            );

            -- vw_CellByPartNo column order: PartNo, Family, Subfamily, Description, CellId, CellName
            -- Both CellByPartNo and CellByPartNoView are populated from this view
            CREATE TABLE IF NOT EXISTS CellByPartNo (
                PartNo VARCHAR NOT NULL,
                Family VARCHAR,
                Subfamily VARCHAR,
                Description VARCHAR,
                CellId INTEGER NOT NULL,
                CellName VARCHAR
            );

            CREATE TABLE IF NOT EXISTS CellByPartNoView (
                PartNo VARCHAR NOT NULL,
                Family VARCHAR,
                Subfamily VARCHAR,
                Description VARCHAR,
                CellId INTEGER NOT NULL,
                CellName VARCHAR
            );
            """;

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
