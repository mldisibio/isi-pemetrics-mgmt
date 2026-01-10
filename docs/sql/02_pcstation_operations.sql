/*
    PE_Metrics Dimension Management - PCStation Operations

    Objects created:
    - mgmt.vw_PCStation         : View for listing all PC stations
    - mgmt.PCStation_Search     : Search PC stations by prefix (for autocomplete)
    - mgmt.PCStation_Insert     : Insert new PC station if not exists

    Business Rules:
    - PcName is the primary key (no surrogate ID needed)
    - PcName must be unique (enforced by PK)

    Notes:
    - Existence check is handled via PCStation_Search: a result count of zero
      means the name does not exist. From the UI, no matches visually confirms
      non-existence; programmatically, an empty resultset indicates the same.
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
-- PROCEDURE: mgmt.PCStation_Insert
-- Purpose: Insert a new PC station if it does not already exist
-- Behavior: Silently succeeds if the name already exists (idempotent)
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.PCStation_Insert', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.PCStation_Insert;
GO

CREATE PROCEDURE mgmt.PCStation_Insert
    @PcName VARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert only if not already exists
    IF NOT EXISTS (SELECT 1 FROM floor.PCStation WHERE PcName = @PcName)
    BEGIN
        INSERT INTO floor.PCStation (PcName)
        VALUES (@PcName);
    END
END
GO

PRINT 'PCStation operations created successfully.';
GO
