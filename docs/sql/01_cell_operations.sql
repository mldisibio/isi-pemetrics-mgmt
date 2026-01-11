/*
    PE_Metrics Dimension Management - Cell Operations

    Objects created:
    - mgmt.vw_Cell          : View for listing all cells (read-only display)
    - mgmt.Cell_GetById     : Get single cell by ID
    - mgmt.Cell_Insert      : Insert new cell, returns identity
    - mgmt.Cell_Update      : Update existing cell

    Business Rules:
    - CellName must be unique (enforced by UQ_floor_Cell_Name)
    - DisplayName must be unique (enforced by UQ_floor_Cell_Display)
    - Cells are never hard deleted; set ActiveTo for soft delete
    - ActiveFrom is required
*/

USE PE_Metrics;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_Cell
-- Purpose: Read-only view of all cells for display in data grids
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
-- PROCEDURE: mgmt.Cell_GetById
-- Purpose: Retrieve a single cell by its ID
-- Returns: Single row or empty if not found
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
    FROM floor.Cell
    WHERE CellId = @CellId;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.Cell_Insert
-- Purpose: Insert a new cell and return the auto-generated identity
-- Returns: The new CellId
-- Errors:
--   50001 - CellName already exists
--   50002 - DisplayName already exists
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

    -- Validate CellName uniqueness
    IF EXISTS (SELECT 1 FROM floor.Cell WHERE CellName = @CellName)
    BEGIN
        RAISERROR(50001, 16, 1, 'CellName already exists.');
        RETURN;
    END

    -- Validate DisplayName uniqueness
    IF EXISTS (SELECT 1 FROM floor.Cell WHERE DisplayName = @DisplayName)
    BEGIN
        RAISERROR(50002, 16, 1, 'DisplayName already exists.');
        RETURN;
    END

    -- Insert the new cell
    INSERT INTO floor.Cell (CellName, DisplayName, ActiveFrom, ActiveTo, Description, AlternativeNames)
    VALUES (@CellName, @DisplayName, @ActiveFrom, @ActiveTo, @Description, @AlternativeNames);

    SET @NewCellId = SCOPE_IDENTITY();
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.Cell_Update
-- Purpose: Update an existing cell
-- Errors:
--   50003 - Cell not found
--   50001 - CellName already exists (for different cell)
--   50002 - DisplayName already exists (for different cell)
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

    -- Validate cell exists
    IF NOT EXISTS (SELECT 1 FROM floor.Cell WHERE CellId = @CellId)
    BEGIN
        RAISERROR(50003, 16, 1, 'Cell not found.');
        RETURN;
    END

    -- Validate CellName uniqueness (excluding current cell)
    IF EXISTS (SELECT 1 FROM floor.Cell WHERE CellName = @CellName AND CellId <> @CellId)
    BEGIN
        RAISERROR(50001, 16, 1, 'CellName already exists.');
        RETURN;
    END

    -- Validate DisplayName uniqueness (excluding current cell)
    IF EXISTS (SELECT 1 FROM floor.Cell WHERE DisplayName = @DisplayName AND CellId <> @CellId)
    BEGIN
        RAISERROR(50002, 16, 1, 'DisplayName already exists.');
        RETURN;
    END

    -- Update the cell
    UPDATE floor.Cell
    SET CellName = @CellName,
        DisplayName = @DisplayName,
        ActiveFrom = @ActiveFrom,
        ActiveTo = @ActiveTo,
        Description = @Description,
        AlternativeNames = @AlternativeNames
    WHERE CellId = @CellId;
END
GO

PRINT 'Cell operations created successfully.';
GO
