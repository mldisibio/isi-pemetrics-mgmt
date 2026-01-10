/*
    PE_Metrics Dimension Management - PCStation Operations

    Objects created:
    - mgmt.vw_PCStation         : View for listing all PC stations
    - mgmt.PCStation_Search     : Search PC stations by prefix (for autocomplete)
    - mgmt.PCStation_Exists     : Check if a PC name exists
    - mgmt.PCStation_Insert     : Insert new PC station if not exists

    Business Rules:
    - PcName is the primary key (no surrogate ID needed)
    - PcName must be unique (enforced by PK)
*/

USE PE_Metrics;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_PCStation
-- Purpose: Read-only view of all PC stations for display
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
-- PROCEDURE: mgmt.PCStation_Search
-- Purpose: Search PC stations by prefix for autocomplete
-- Returns: Matching PC names sorted alphabetically
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.PCStation_Search', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.PCStation_Search;
GO

CREATE PROCEDURE mgmt.PCStation_Search
    @SearchPrefix VARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PcName
    FROM floor.PCStation
    WHERE PcName LIKE @SearchPrefix + '%'
    ORDER BY PcName;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.PCStation_Exists
-- Purpose: Check if a PC name already exists
-- Returns: 1 if exists, 0 if not
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.PCStation_Exists', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.PCStation_Exists;
GO

CREATE PROCEDURE mgmt.PCStation_Exists
    @PcName VARCHAR(128),
    @Exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM floor.PCStation WHERE PcName = @PcName)
        SET @Exists = 1;
    ELSE
        SET @Exists = 0;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.PCStation_Insert
-- Purpose: Insert a new PC station
-- Errors:
--   50010 - PcName already exists
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.PCStation_Insert', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.PCStation_Insert;
GO

CREATE PROCEDURE mgmt.PCStation_Insert
    @PcName VARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate PcName uniqueness
    IF EXISTS (SELECT 1 FROM floor.PCStation WHERE PcName = @PcName)
    BEGIN
        RAISERROR(50010, 16, 1, 'PC station name already exists.');
        RETURN;
    END

    -- Insert the new PC station
    INSERT INTO floor.PCStation (PcName)
    VALUES (@PcName);
END
GO

PRINT 'PCStation operations created successfully.';
GO
