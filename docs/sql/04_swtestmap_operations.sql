/*
    PE_Metrics Dimension Management - SwTestMap Operations

    Objects created:
    - mgmt.vw_SwTestMap         : View for listing all software tests
    - mgmt.SwTestMap_GetById    : Get single test by ID
    - mgmt.SwTestMap_Insert     : Insert new test, returns identity
    - mgmt.SwTestMap_Update     : Update existing test

    Business Rules:
    - Software tests are never hard deleted
    - LastRun suggests if a test is obsolete (used for "active" filtering)
    - A test is considered "active" if LastRun is NULL or within past 3 months
    - The combination of (ConfiguredTestId, TestName) must be unique
      (These values from raw transactional data are mapped to ReportKey for
      metrics reporting; the ETL uses them to find the correct ReportKey)
*/

USE PE_Metrics;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_SwTestMap
-- Purpose: Read-only view of all software tests for display
-- Includes IsActive flag (LastRun is NULL or within past 3 months)
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
-- PROCEDURE: mgmt.SwTestMap_GetById
-- Purpose: Retrieve a single software test by its ID
-- Returns: Single row or empty if not found
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
    FROM sw.SwTestMap
    WHERE SwTestMapId = @SwTestMapId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.SwTestMap_Insert
-- Purpose: Insert a new software test and return the auto-generated identity
-- Returns: The new SwTestMapId via OUTPUT parameter
-- Errors:
--   50032 - Duplicate (ConfiguredTestId, TestName) combination
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

    -- Validate unique (ConfiguredTestId, TestName) combination
    IF EXISTS (SELECT 1 FROM sw.SwTestMap
               WHERE ConfiguredTestId = @ConfiguredTestId AND TestName = @TestName)
    BEGIN
        RAISERROR(50032, 16, 1, 'A software test with this ConfiguredTestId and TestName already exists.');
        RETURN;
    END

    -- Insert the new software test
    INSERT INTO sw.SwTestMap (ConfiguredTestId, TestApplication, TestName, ReportKey,
                              TestDirectory, RelativePath, LastRun, Notes)
    VALUES (@ConfiguredTestId, @TestApplication, @TestName, @ReportKey,
            @TestDirectory, @RelativePath, @LastRun, @Notes);

    SET @NewSwTestMapId = SCOPE_IDENTITY();
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.SwTestMap_Update
-- Purpose: Update an existing software test
-- Errors:
--   50030 - Software test not found
--   50032 - Duplicate (ConfiguredTestId, TestName) combination
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

    -- Validate test exists
    IF NOT EXISTS (SELECT 1 FROM sw.SwTestMap WHERE SwTestMapId = @SwTestMapId)
    BEGIN
        RAISERROR(50030, 16, 1, 'Software test not found.');
        RETURN;
    END

    -- Validate unique (ConfiguredTestId, TestName) combination (excluding current record)
    IF EXISTS (SELECT 1 FROM sw.SwTestMap
               WHERE ConfiguredTestId = @ConfiguredTestId AND TestName = @TestName
               AND SwTestMapId <> @SwTestMapId)
    BEGIN
        RAISERROR(50032, 16, 1, 'A software test with this ConfiguredTestId and TestName already exists.');
        RETURN;
    END

    -- Update the software test
    UPDATE sw.SwTestMap
    SET ConfiguredTestId = @ConfiguredTestId,
        TestApplication = @TestApplication,
        TestName = @TestName,
        ReportKey = @ReportKey,
        TestDirectory = @TestDirectory,
        RelativePath = @RelativePath,
        LastRun = @LastRun,
        Notes = @Notes
    WHERE SwTestMapId = @SwTestMapId;
END
GO

PRINT 'SwTestMap operations created successfully.';
GO
