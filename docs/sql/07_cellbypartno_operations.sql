/*
    PE_Metrics Dimension Management - CellByPartNo Operations

    Objects created:
    - mgmt.CellByPartNo_GetByPartNo         : Get all cell mappings for a part number
    - mgmt.CellByPartNo_SetMappings         : Replace all cell mappings for a part number
    - mgmt.CellByPartNo_AddMapping          : Add a single cell mapping (idempotent)
    - mgmt.CellByPartNo_DeleteMapping       : Remove a single cell mapping (idempotent)

    Business Rules:
    - A part number can be associated with one or more cells
    - This is a simple join table with composite primary key (CellId, PartNo)
    - Mappings can be managed as a set (SetMappings) or individually (Add/Delete)
*/

USE PE_Metrics;
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPartNo_GetByPartNo
-- Purpose: Get all cell mappings for a specific part number
-- Returns: List of cells with their names
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellByPartNo_GetByPartNo', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellByPartNo_GetByPartNo;
GO

CREATE PROCEDURE mgmt.CellByPartNo_GetByPartNo
    @PartNo VARCHAR(32)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.CellId,
        m.PartNo,
        c.CellName
    FROM floor.CellByPartNo m
    INNER JOIN floor.Cell c ON m.CellId = c.CellId
    WHERE m.PartNo = @PartNo
    ORDER BY c.CellName;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPartNo_SetMappings
-- Purpose: Replace all cell mappings for a part number
-- This deletes existing mappings and inserts new ones in a single transaction
-- Parameters:
--   @PartNo  - The part number
--   @CellIds - Table-valued parameter with the list of CellIds to assign
-- Errors:
--   50041 - Part number not found
--   50031 - One or more cells not found
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

    -- Validate part number exists
    IF NOT EXISTS (SELECT 1 FROM product.TLA WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50041, 16, 1, 'Part number not found.');
        RETURN;
    END

    -- Validate all provided CellIds exist
    IF EXISTS (
        SELECT 1 FROM @CellIds ci
        WHERE NOT EXISTS (SELECT 1 FROM floor.Cell c WHERE c.CellId = ci.Value)
    )
    BEGIN
        RAISERROR(50031, 16, 1, 'One or more specified cells were not found.');
        RETURN;
    END

    BEGIN TRANSACTION;

    BEGIN TRY
        -- Delete existing mappings
        DELETE FROM floor.CellByPartNo
        WHERE PartNo = @PartNo;

        -- Insert new mappings
        INSERT INTO floor.CellByPartNo (CellId, PartNo)
        SELECT Value, @PartNo
        FROM @CellIds;

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
-- Purpose: Add a single cell mapping for a part number
-- Behavior: Silently succeeds if the mapping already exists (idempotent)
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

    -- Insert only if not already exists
    IF NOT EXISTS (SELECT 1 FROM floor.CellByPartNo
                   WHERE PartNo = @PartNo AND CellId = @CellId)
    BEGIN
        INSERT INTO floor.CellByPartNo (CellId, PartNo)
        VALUES (@CellId, @PartNo);
    END
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellByPartNo_DeleteMapping
-- Purpose: Remove a single cell mapping for a part number
-- Behavior: Silently succeeds if the mapping does not exist (idempotent)
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

    DELETE FROM floor.CellByPartNo
    WHERE PartNo = @PartNo AND CellId = @CellId;
END
GO

PRINT 'CellByPartNo operations created successfully.';
GO
