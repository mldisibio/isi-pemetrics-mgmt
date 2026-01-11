/*
    PE_Metrics Dimension Management - CellByPCStation Operations

    Objects created:
    - mgmt.vw_CellByPCStation       : View for listing all PC-to-Cell mappings with Cell names
    - mgmt.CellByPCStation_GetById  : Get single mapping by ID
    - mgmt.CellByPCStation_Insert   : Insert new mapping, returns identity
    - mgmt.CellByPCStation_Update   : Update existing mapping

    Business Rules:
    - A PC is allocated to one cell for a given time period
    - Mappings are never hard deleted; set ActiveTo for soft delete
    - CellId must reference an existing Cell
    - PcName must reference an existing PCStation
    - Unique constraint on (CellId, PcName, ActiveFrom)
*/

USE PE_Metrics;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_CellByPCStation
-- Purpose: Read-only view of all PC-to-Cell mappings with Cell names
-- Includes IsActive flag for filtering
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
-- PROCEDURE: mgmt.CellByPCStation_GetById
-- Purpose: Retrieve a single mapping by its ID
-- Returns: Single row or empty if not found
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
    INNER JOIN floor.Cell c ON m.CellId = c.CellId
    WHERE m.StationMapId = @StationMapId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPCStation_Insert
-- Purpose: Insert a new PC-to-Cell mapping and return the auto-generated identity
-- Returns: The new StationMapId via OUTPUT parameter
-- Errors:
--   50020 - Cell not found
--   50021 - PC station not found
--   50022 - Duplicate mapping (same CellId, PcName, ActiveFrom)
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

    -- Validate Cell exists
    IF NOT EXISTS (SELECT 1 FROM floor.Cell WHERE CellId = @CellId)
    BEGIN
        RAISERROR(50020, 16, 1, 'Cell not found.');
        RETURN;
    END

    -- Validate PCStation exists
    IF NOT EXISTS (SELECT 1 FROM floor.PCStation WHERE PcName = @PcName)
    BEGIN
        RAISERROR(50021, 16, 1, 'PC station not found.');
        RETURN;
    END

    -- Check for duplicate mapping
    IF EXISTS (SELECT 1 FROM floor.CellByPCStation
               WHERE CellId = @CellId AND PcName = @PcName AND ActiveFrom = @ActiveFrom)
    BEGIN
        RAISERROR(50022, 16, 1, 'A mapping for this Cell, PC, and ActiveFrom date already exists.');
        RETURN;
    END

    -- Insert the new mapping
    INSERT INTO floor.CellByPCStation (CellId, PcName, PcPurpose, ActiveFrom, ActiveTo, ExtendedName)
    VALUES (@CellId, @PcName, @PcPurpose, @ActiveFrom, @ActiveTo, @ExtendedName);

    SET @NewStationMapId = SCOPE_IDENTITY();
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPCStation_Update
-- Purpose: Update an existing PC-to-Cell mapping
-- Errors:
--   50023 - Mapping not found
--   50020 - Cell not found
--   50021 - PC station not found
--   50022 - Duplicate mapping (same CellId, PcName, ActiveFrom for different record)
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

    -- Validate mapping exists
    IF NOT EXISTS (SELECT 1 FROM floor.CellByPCStation WHERE StationMapId = @StationMapId)
    BEGIN
        RAISERROR(50023, 16, 1, 'Mapping not found.');
        RETURN;
    END

    -- Validate Cell exists
    IF NOT EXISTS (SELECT 1 FROM floor.Cell WHERE CellId = @CellId)
    BEGIN
        RAISERROR(50020, 16, 1, 'Cell not found.');
        RETURN;
    END

    -- Validate PCStation exists
    IF NOT EXISTS (SELECT 1 FROM floor.PCStation WHERE PcName = @PcName)
    BEGIN
        RAISERROR(50021, 16, 1, 'PC station not found.');
        RETURN;
    END

    -- Check for duplicate mapping (excluding current record)
    IF EXISTS (SELECT 1 FROM floor.CellByPCStation
               WHERE CellId = @CellId AND PcName = @PcName AND ActiveFrom = @ActiveFrom
               AND StationMapId <> @StationMapId)
    BEGIN
        RAISERROR(50022, 16, 1, 'A mapping for this Cell, PC, and ActiveFrom date already exists.');
        RETURN;
    END

    -- Update the mapping
    UPDATE floor.CellByPCStation
    SET CellId = @CellId,
        PcName = @PcName,
        PcPurpose = @PcPurpose,
        ActiveFrom = @ActiveFrom,
        ActiveTo = @ActiveTo,
        ExtendedName = @ExtendedName
    WHERE StationMapId = @StationMapId;
END
GO

PRINT 'CellByPCStation operations created successfully.';
GO
