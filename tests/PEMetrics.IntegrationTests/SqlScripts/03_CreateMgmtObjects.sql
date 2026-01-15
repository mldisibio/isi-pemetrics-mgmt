-- Combined management objects script for testing
-- Creates mgmt schema, views, stored procedures, and types

--------------------------------------------------------------------------------
-- SCHEMA: mgmt
--------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'mgmt')
BEGIN
    EXEC('CREATE SCHEMA mgmt');
END
GO

--------------------------------------------------------------------------------
-- TYPE: mgmt.IntList (table-valued parameter for batch operations)
--------------------------------------------------------------------------------
IF TYPE_ID('mgmt.IntList') IS NULL
BEGIN
    CREATE TYPE mgmt.IntList AS TABLE (
        Value INT NOT NULL PRIMARY KEY
    );
END
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_Cell
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.vw_Cell', 'V') IS NOT NULL
    DROP VIEW mgmt.vw_Cell;
GO

CREATE VIEW mgmt.vw_Cell
AS
SELECT
    CellId,
    CellName,
    DisplayName,
    ActiveFrom,
    ActiveTo,
    Description,
    AlternativeNames,
    CASE
        WHEN ActiveTo IS NULL OR ActiveTo >= CAST(GETDATE() AS DATE)
        THEN 1
        ELSE 0
    END AS IsActive
FROM floor.Cell;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_PCStation
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.vw_PCStation', 'V') IS NOT NULL
    DROP VIEW mgmt.vw_PCStation;
GO

CREATE VIEW mgmt.vw_PCStation
AS
SELECT
    PcName
FROM floor.PCStation;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_CellByPCStation
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.vw_CellByPCStation', 'V') IS NOT NULL
    DROP VIEW mgmt.vw_CellByPCStation;
GO

CREATE VIEW mgmt.vw_CellByPCStation
AS
SELECT
    m.StationMapId,
    m.CellId,
    c.CellName,
    m.PcName,
    m.PcPurpose,
    m.ActiveFrom,
    m.ActiveTo,
    m.ExtendedName,
    CASE
        WHEN m.ActiveTo IS NULL OR m.ActiveTo >= CAST(GETDATE() AS DATE)
        THEN 1
        ELSE 0
    END AS IsActive
FROM floor.CellByPCStation m
INNER JOIN floor.Cell c ON m.CellId = c.CellId;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_SwTestMap
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.vw_SwTestMap', 'V') IS NOT NULL
    DROP VIEW mgmt.vw_SwTestMap;
GO

CREATE VIEW mgmt.vw_SwTestMap
AS
SELECT
    SwTestMapId,
    ConfiguredTestId,
    TestApplication,
    TestName,
    ReportKey,
    TestDirectory,
    RelativePath,
    LastRun,
    Notes,
    CASE
        WHEN LastRun IS NULL OR LastRun >= DATEADD(MONTH, -3, CAST(GETDATE() AS DATE))
        THEN 1
        ELSE 0
    END AS IsActive
FROM sw.SwTestMap;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_CellBySwTest
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.vw_CellBySwTest', 'V') IS NOT NULL
    DROP VIEW mgmt.vw_CellBySwTest;
GO

CREATE VIEW mgmt.vw_CellBySwTest
AS
SELECT
    m.SwTestMapId,
    sw.ConfiguredTestId,
    sw.TestApplication,
    sw.ReportKey,
    sw.LastRun,
    CASE
        WHEN LastRun IS NULL OR LastRun >= DATEADD(MONTH, -3, CAST(GETDATE() AS DATE))
        THEN 1
        ELSE 0
    END AS IsActive,
    m.CellId,
    c.CellName
FROM floor.CellBySwTest m
INNER JOIN sw.SwTestMap sw ON m.SwTestMapId = sw.SwTestMapId
INNER JOIN floor.Cell c ON m.CellId = c.CellId;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_TLA
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.vw_TLA', 'V') IS NOT NULL
    DROP VIEW mgmt.vw_TLA;
GO

CREATE VIEW mgmt.vw_TLA
AS
SELECT
    t.PartNo,
    t.Family,
    t.Subfamily,
    t.ServiceGroup,
    t.FormalDescription,
    t.Description,
    CASE
        WHEN EXISTS (SELECT 1 FROM activity.ProductionTest pt WHERE pt.PartNo = t.PartNo)
        THEN 1
        ELSE 0
    END AS IsUsed
FROM product.TLA t;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_CellByPartNo
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.vw_CellByPartNo', 'V') IS NOT NULL
    DROP VIEW mgmt.vw_CellByPartNo;
GO

CREATE VIEW mgmt.vw_CellByPartNo
AS
SELECT
    m.PartNo,
    tla.Family,
    tla.Subfamily,
    tla.Description,
    m.CellId,
    c.CellName
FROM floor.CellByPartNo m
INNER JOIN product.TLA tla ON m.PartNo = tla.PartNo
INNER JOIN floor.Cell c ON m.CellId = c.CellId;
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.Cell_GetById
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.Cell_GetById', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.Cell_GetById;
GO

CREATE PROCEDURE mgmt.Cell_GetById
    @CellId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        CellId, CellName, DisplayName, ActiveFrom, ActiveTo, Description, AlternativeNames,
        CASE WHEN ActiveTo IS NULL OR ActiveTo >= CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END AS IsActive
    FROM floor.Cell
    WHERE CellId = @CellId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.Cell_Insert
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.Cell_Insert', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.Cell_Insert;
GO

CREATE PROCEDURE mgmt.Cell_Insert
    @CellName         VARCHAR(64),
    @DisplayName      VARCHAR(128),
    @ActiveFrom       DATE,
    @ActiveTo         DATE = NULL,
    @Description      VARCHAR(1024) = NULL,
    @AlternativeNames VARCHAR(1024) = NULL,
    @NewCellId        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM floor.Cell WHERE CellName = @CellName)
    BEGIN
        RAISERROR(50001, 16, 1, 'CellName already exists.');
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM floor.Cell WHERE DisplayName = @DisplayName)
    BEGIN
        RAISERROR(50002, 16, 1, 'DisplayName already exists.');
        RETURN;
    END
    INSERT INTO floor.Cell (CellName, DisplayName, ActiveFrom, ActiveTo, Description, AlternativeNames)
    VALUES (@CellName, @DisplayName, @ActiveFrom, @ActiveTo, @Description, @AlternativeNames);
    SET @NewCellId = SCOPE_IDENTITY();
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.Cell_Update
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.Cell_Update', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.Cell_Update;
GO

CREATE PROCEDURE mgmt.Cell_Update
    @CellId           INT,
    @CellName         VARCHAR(64),
    @DisplayName      VARCHAR(128),
    @ActiveFrom       DATE,
    @ActiveTo         DATE = NULL,
    @Description      VARCHAR(1024) = NULL,
    @AlternativeNames VARCHAR(1024) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM floor.Cell WHERE CellId = @CellId)
    BEGIN
        RAISERROR(50003, 16, 1, 'Cell not found.');
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM floor.Cell WHERE CellName = @CellName AND CellId <> @CellId)
    BEGIN
        RAISERROR(50001, 16, 1, 'CellName already exists.');
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM floor.Cell WHERE DisplayName = @DisplayName AND CellId <> @CellId)
    BEGIN
        RAISERROR(50002, 16, 1, 'DisplayName already exists.');
        RETURN;
    END
    UPDATE floor.Cell
    SET CellName = @CellName, DisplayName = @DisplayName, ActiveFrom = @ActiveFrom,
        ActiveTo = @ActiveTo, Description = @Description, AlternativeNames = @AlternativeNames
    WHERE CellId = @CellId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.PCStation_Search
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.PCStation_Search', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.PCStation_Search;
GO

CREATE PROCEDURE mgmt.PCStation_Search
    @SearchPrefix VARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PcName FROM floor.PCStation WHERE PcName LIKE @SearchPrefix + '%' ORDER BY PcName;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.PCStation_Insert
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.PCStation_Insert', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.PCStation_Insert;
GO

CREATE PROCEDURE mgmt.PCStation_Insert
    @PcName VARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM floor.PCStation WHERE PcName = @PcName)
    BEGIN
        INSERT INTO floor.PCStation (PcName) VALUES (@PcName);
    END
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPCStation_GetById
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellByPCStation_GetById', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellByPCStation_GetById;
GO

CREATE PROCEDURE mgmt.CellByPCStation_GetById
    @StationMapId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        m.StationMapId, m.CellId, c.CellName, m.PcName, m.PcPurpose, m.ActiveFrom, m.ActiveTo, m.ExtendedName,
        CASE WHEN m.ActiveTo IS NULL OR m.ActiveTo >= CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END AS IsActive
    FROM floor.CellByPCStation m
    INNER JOIN floor.Cell c ON m.CellId = c.CellId
    WHERE m.StationMapId = @StationMapId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPCStation_Insert
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellByPCStation_Insert', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellByPCStation_Insert;
GO

CREATE PROCEDURE mgmt.CellByPCStation_Insert
    @CellId         INT,
    @PcName         VARCHAR(128),
    @PcPurpose      VARCHAR(256) = NULL,
    @ActiveFrom     DATE,
    @ActiveTo       DATE = NULL,
    @ExtendedName   VARCHAR(256) = NULL,
    @NewStationMapId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM floor.Cell WHERE CellId = @CellId)
    BEGIN
        RAISERROR(50020, 16, 1, 'Cell not found.');
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM floor.PCStation WHERE PcName = @PcName)
    BEGIN
        RAISERROR(50021, 16, 1, 'PC station not found.');
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM floor.CellByPCStation WHERE CellId = @CellId AND PcName = @PcName AND ActiveFrom = @ActiveFrom)
    BEGIN
        RAISERROR(50022, 16, 1, 'A mapping for this Cell, PC, and ActiveFrom date already exists.');
        RETURN;
    END
    INSERT INTO floor.CellByPCStation (CellId, PcName, PcPurpose, ActiveFrom, ActiveTo, ExtendedName)
    VALUES (@CellId, @PcName, @PcPurpose, @ActiveFrom, @ActiveTo, @ExtendedName);
    SET @NewStationMapId = SCOPE_IDENTITY();
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPCStation_Update
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellByPCStation_Update', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellByPCStation_Update;
GO

CREATE PROCEDURE mgmt.CellByPCStation_Update
    @StationMapId   INT,
    @CellId         INT,
    @PcName         VARCHAR(128),
    @PcPurpose      VARCHAR(256) = NULL,
    @ActiveFrom     DATE,
    @ActiveTo       DATE = NULL,
    @ExtendedName   VARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM floor.CellByPCStation WHERE StationMapId = @StationMapId)
    BEGIN
        RAISERROR(50023, 16, 1, 'Mapping not found.');
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM floor.Cell WHERE CellId = @CellId)
    BEGIN
        RAISERROR(50020, 16, 1, 'Cell not found.');
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM floor.PCStation WHERE PcName = @PcName)
    BEGIN
        RAISERROR(50021, 16, 1, 'PC station not found.');
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM floor.CellByPCStation WHERE CellId = @CellId AND PcName = @PcName AND ActiveFrom = @ActiveFrom AND StationMapId <> @StationMapId)
    BEGIN
        RAISERROR(50022, 16, 1, 'A mapping for this Cell, PC, and ActiveFrom date already exists.');
        RETURN;
    END
    UPDATE floor.CellByPCStation
    SET CellId = @CellId, PcName = @PcName, PcPurpose = @PcPurpose, ActiveFrom = @ActiveFrom, ActiveTo = @ActiveTo, ExtendedName = @ExtendedName
    WHERE StationMapId = @StationMapId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.SwTestMap_GetById
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.SwTestMap_GetById', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.SwTestMap_GetById;
GO

CREATE PROCEDURE mgmt.SwTestMap_GetById
    @SwTestMapId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        SwTestMapId, ConfiguredTestId, TestApplication, TestName, ReportKey, TestDirectory, RelativePath, LastRun, Notes,
        CASE WHEN LastRun IS NULL OR LastRun >= DATEADD(MONTH, -3, CAST(GETDATE() AS DATE)) THEN 1 ELSE 0 END AS IsActive
    FROM sw.SwTestMap
    WHERE SwTestMapId = @SwTestMapId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.SwTestMap_Insert
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.SwTestMap_Insert', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.SwTestMap_Insert;
GO

CREATE PROCEDURE mgmt.SwTestMap_Insert
    @ConfiguredTestId VARCHAR(16) = NULL,
    @TestApplication  VARCHAR(128) = NULL,
    @TestName         VARCHAR(256) = NULL,
    @ReportKey        VARCHAR(128) = NULL,
    @TestDirectory    VARCHAR(256) = NULL,
    @RelativePath     VARCHAR(256) = NULL,
    @LastRun          DATE = NULL,
    @Notes            VARCHAR(256) = NULL,
    @NewSwTestMapId   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM sw.SwTestMap WHERE ConfiguredTestId = @ConfiguredTestId AND TestName = @TestName)
    BEGIN
        RAISERROR(50032, 16, 1, 'A software test with this ConfiguredTestId and TestName already exists.');
        RETURN;
    END
    INSERT INTO sw.SwTestMap (ConfiguredTestId, TestApplication, TestName, ReportKey, TestDirectory, RelativePath, LastRun, Notes)
    VALUES (@ConfiguredTestId, @TestApplication, @TestName, @ReportKey, @TestDirectory, @RelativePath, @LastRun, @Notes);
    SET @NewSwTestMapId = SCOPE_IDENTITY();
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.SwTestMap_Update
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.SwTestMap_Update', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.SwTestMap_Update;
GO

CREATE PROCEDURE mgmt.SwTestMap_Update
    @SwTestMapId      INT,
    @ConfiguredTestId VARCHAR(16) = NULL,
    @TestApplication  VARCHAR(128) = NULL,
    @TestName         VARCHAR(256) = NULL,
    @ReportKey        VARCHAR(128) = NULL,
    @TestDirectory    VARCHAR(256) = NULL,
    @RelativePath     VARCHAR(256) = NULL,
    @LastRun          DATE = NULL,
    @Notes            VARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM sw.SwTestMap WHERE SwTestMapId = @SwTestMapId)
    BEGIN
        RAISERROR(50030, 16, 1, 'Software test not found.');
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM sw.SwTestMap WHERE ConfiguredTestId = @ConfiguredTestId AND TestName = @TestName AND SwTestMapId <> @SwTestMapId)
    BEGIN
        RAISERROR(50032, 16, 1, 'A software test with this ConfiguredTestId and TestName already exists.');
        RETURN;
    END
    UPDATE sw.SwTestMap
    SET ConfiguredTestId = @ConfiguredTestId, TestApplication = @TestApplication, TestName = @TestName, ReportKey = @ReportKey,
        TestDirectory = @TestDirectory, RelativePath = @RelativePath, LastRun = @LastRun, Notes = @Notes
    WHERE SwTestMapId = @SwTestMapId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellBySwTest_GetBySwTestMapId
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellBySwTest_GetBySwTestMapId', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellBySwTest_GetBySwTestMapId;
GO

CREATE PROCEDURE mgmt.CellBySwTest_GetBySwTestMapId
    @SwTestMapId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.SwTestMapId, m.CellId, c.CellName
    FROM floor.CellBySwTest m
    INNER JOIN floor.Cell c ON m.CellId = c.CellId
    WHERE m.SwTestMapId = @SwTestMapId
    ORDER BY c.CellName;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellBySwTest_SetMappings
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellBySwTest_SetMappings', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellBySwTest_SetMappings;
GO

CREATE PROCEDURE mgmt.CellBySwTest_SetMappings
    @SwTestMapId INT,
    @CellIds     mgmt.IntList READONLY
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM sw.SwTestMap WHERE SwTestMapId = @SwTestMapId)
    BEGIN
        RAISERROR(50030, 16, 1, 'Software test not found.');
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM @CellIds ci WHERE NOT EXISTS (SELECT 1 FROM floor.Cell c WHERE c.CellId = ci.Value))
    BEGIN
        RAISERROR(50031, 16, 1, 'One or more specified cells were not found.');
        RETURN;
    END
    BEGIN TRANSACTION;
    BEGIN TRY
        DELETE FROM floor.CellBySwTest WHERE SwTestMapId = @SwTestMapId;
        INSERT INTO floor.CellBySwTest (CellId, SwTestMapId) SELECT Value, @SwTestMapId FROM @CellIds;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellBySwTest_AddMapping
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellBySwTest_AddMapping', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellBySwTest_AddMapping;
GO

CREATE PROCEDURE mgmt.CellBySwTest_AddMapping
    @SwTestMapId INT,
    @CellId      INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM sw.SwTestMap WHERE SwTestMapId = @SwTestMapId)
    BEGIN
        RAISERROR(50030, 16, 1, 'Software test not found.');
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM floor.Cell WHERE CellId = @CellId)
    BEGIN
        RAISERROR(50031, 16, 1, 'Cell not found.');
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM floor.CellBySwTest WHERE SwTestMapId = @SwTestMapId AND CellId = @CellId)
    BEGIN
        INSERT INTO floor.CellBySwTest (CellId, SwTestMapId) VALUES (@CellId, @SwTestMapId);
    END
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellBySwTest_DeleteMapping
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellBySwTest_DeleteMapping', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellBySwTest_DeleteMapping;
GO

CREATE PROCEDURE mgmt.CellBySwTest_DeleteMapping
    @SwTestMapId INT,
    @CellId      INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM floor.CellBySwTest WHERE SwTestMapId = @SwTestMapId AND CellId = @CellId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.TLA_GetByPartNo
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.TLA_GetByPartNo', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.TLA_GetByPartNo;
GO

CREATE PROCEDURE mgmt.TLA_GetByPartNo
    @PartNo VARCHAR(32)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        t.PartNo, t.Family, t.Subfamily, t.ServiceGroup, t.FormalDescription, t.Description,
        CASE WHEN EXISTS (SELECT 1 FROM activity.ProductionTest pt WHERE pt.PartNo = t.PartNo) THEN 1 ELSE 0 END AS IsUsed
    FROM product.TLA t
    WHERE t.PartNo = @PartNo;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.TLA_Insert
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.TLA_Insert', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.TLA_Insert;
GO

CREATE PROCEDURE mgmt.TLA_Insert
    @PartNo            VARCHAR(32),
    @Family            VARCHAR(64) = NULL,
    @Subfamily         VARCHAR(96) = NULL,
    @ServiceGroup      VARCHAR(32) = NULL,
    @FormalDescription VARCHAR(256) = NULL,
    @Description       VARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM product.TLA WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50040, 16, 1, 'Part number already exists.');
        RETURN;
    END
    INSERT INTO product.TLA (PartNo, Family, Subfamily, ServiceGroup, FormalDescription, Description)
    VALUES (@PartNo, @Family, @Subfamily, @ServiceGroup, @FormalDescription, @Description);
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.TLA_Update
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.TLA_Update', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.TLA_Update;
GO

CREATE PROCEDURE mgmt.TLA_Update
    @PartNo            VARCHAR(32),
    @Family            VARCHAR(64) = NULL,
    @Subfamily         VARCHAR(96) = NULL,
    @ServiceGroup      VARCHAR(32) = NULL,
    @FormalDescription VARCHAR(256) = NULL,
    @Description       VARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM product.TLA WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50041, 16, 1, 'Part number not found.');
        RETURN;
    END
    UPDATE product.TLA
    SET Family = @Family, Subfamily = @Subfamily, ServiceGroup = @ServiceGroup,
        FormalDescription = @FormalDescription, Description = @Description
    WHERE PartNo = @PartNo;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.TLA_Delete
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.TLA_Delete', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.TLA_Delete;
GO

CREATE PROCEDURE mgmt.TLA_Delete
    @PartNo VARCHAR(32)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM product.TLA WHERE PartNo = @PartNo)
        RETURN;
    IF EXISTS (SELECT 1 FROM activity.ProductionTest WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50042, 16, 1, 'Cannot delete: Part number is referenced in production tests.');
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM floor.CellByPartNo WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50043, 16, 1, 'Cannot delete: Part number is referenced in cell mappings. Remove cell mappings first.');
        RETURN;
    END
    DELETE FROM product.TLA WHERE PartNo = @PartNo;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPartNo_GetByPartNo
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellByPartNo_GetByPartNo', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellByPartNo_GetByPartNo;
GO

CREATE PROCEDURE mgmt.CellByPartNo_GetByPartNo
    @PartNo VARCHAR(32)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.CellId, m.PartNo, c.CellName
    FROM floor.CellByPartNo m
    INNER JOIN floor.Cell c ON m.CellId = c.CellId
    WHERE m.PartNo = @PartNo
    ORDER BY c.CellName;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPartNo_SetMappings
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellByPartNo_SetMappings', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellByPartNo_SetMappings;
GO

CREATE PROCEDURE mgmt.CellByPartNo_SetMappings
    @PartNo  VARCHAR(32),
    @CellIds mgmt.IntList READONLY
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM product.TLA WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50041, 16, 1, 'Part number not found.');
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM @CellIds ci WHERE NOT EXISTS (SELECT 1 FROM floor.Cell c WHERE c.CellId = ci.Value))
    BEGIN
        RAISERROR(50031, 16, 1, 'One or more specified cells were not found.');
        RETURN;
    END
    BEGIN TRANSACTION;
    BEGIN TRY
        DELETE FROM floor.CellByPartNo WHERE PartNo = @PartNo;
        INSERT INTO floor.CellByPartNo (CellId, PartNo) SELECT Value, @PartNo FROM @CellIds;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPartNo_AddMapping
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellByPartNo_AddMapping', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellByPartNo_AddMapping;
GO

CREATE PROCEDURE mgmt.CellByPartNo_AddMapping
    @PartNo VARCHAR(32),
    @CellId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM product.TLA WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50041, 16, 1, 'Part number not found.');
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM floor.Cell WHERE CellId = @CellId)
    BEGIN
        RAISERROR(50031, 16, 1, 'Cell not found.');
        RETURN;
    END
    IF NOT EXISTS (SELECT 1 FROM floor.CellByPartNo WHERE PartNo = @PartNo AND CellId = @CellId)
    BEGIN
        INSERT INTO floor.CellByPartNo (CellId, PartNo) VALUES (@CellId, @PartNo);
    END
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPartNo_DeleteMapping
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellByPartNo_DeleteMapping', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellByPartNo_DeleteMapping;
GO

CREATE PROCEDURE mgmt.CellByPartNo_DeleteMapping
    @PartNo VARCHAR(32),
    @CellId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM floor.CellByPartNo WHERE PartNo = @PartNo AND CellId = @CellId;
END
GO
