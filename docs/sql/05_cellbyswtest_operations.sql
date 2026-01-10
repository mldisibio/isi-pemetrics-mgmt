/*
    PE_Metrics Dimension Management - CellBySwTest Operations

    Objects created:
    - mgmt.IntList (TYPE)                   : Table-valued parameter type for passing lists of integers
    - mgmt.CellBySwTest_GetBySwTestMapId    : Get all cell mappings for a software test
    - mgmt.CellBySwTest_SetMappings         : Replace all cell mappings for a software test
    - mgmt.CellBySwTest_AddMapping          : Add a single cell mapping (idempotent)
    - mgmt.CellBySwTest_DeleteMapping       : Remove a single cell mapping (idempotent)

    Business Rules:
    - A software test can be assigned to one or more cells
    - This is a simple join table with composite primary key (CellId, SwTestMapId)
    - Mappings can be managed as a set (SetMappings) or individually (Add/Delete)
*/

USE PE_Metrics;
GO

--------------------------------------------------------------------------------
-- TYPE: mgmt.IntList
-- Purpose: Table-valued parameter type for passing lists of integers
-- Used for batch operations like setting multiple cell mappings
--------------------------------------------------------------------------------
IF TYPE_ID('mgmt.IntList') IS NOT NULL
BEGIN
    -- Check if it's being used by any procedures first
    -- Drop and recreate if needed
    IF NOT EXISTS (
        SELECT 1 FROM sys.parameters p
        INNER JOIN sys.types t ON p.user_type_id = t.user_type_id
        WHERE t.name = 'IntList' AND t.schema_id = SCHEMA_ID('mgmt')
    )
    BEGIN
        DROP TYPE mgmt.IntList;
    END
END
GO

IF TYPE_ID('mgmt.IntList') IS NULL
BEGIN
    CREATE TYPE mgmt.IntList AS TABLE (
        Value INT NOT NULL PRIMARY KEY
    );
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellBySwTest_GetBySwTestMapId
-- Purpose: Get all cell mappings for a specific software test
-- Returns: List of cells with their names
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.CellBySwTest_GetBySwTestMapId', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.CellBySwTest_GetBySwTestMapId;
GO

CREATE PROCEDURE mgmt.CellBySwTest_GetBySwTestMapId
    @SwTestMapId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.SwTestMapId,
        m.CellId,
        c.CellName
    FROM floor.CellBySwTest m
    INNER JOIN floor.Cell c ON m.CellId = c.CellId
    WHERE m.SwTestMapId = @SwTestMapId
    ORDER BY c.CellName;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellBySwTest_SetMappings
-- Purpose: Replace all cell mappings for a software test
-- This deletes existing mappings and inserts new ones in a single transaction
-- Parameters:
--   @SwTestMapId - The software test ID
--   @CellIds     - Table-valued parameter with the list of CellIds to assign
-- Errors:
--   50030 - Software test not found
--   50031 - One or more cells not found
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

    -- Validate software test exists
    IF NOT EXISTS (SELECT 1 FROM sw.SwTestMap WHERE SwTestMapId = @SwTestMapId)
    BEGIN
        RAISERROR(50030, 16, 1, 'Software test not found.');
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
        DELETE FROM floor.CellBySwTest
        WHERE SwTestMapId = @SwTestMapId;

        -- Insert new mappings
        INSERT INTO floor.CellBySwTest (CellId, SwTestMapId)
        SELECT Value, @SwTestMapId
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
-- PROCEDURE: mgmt.CellBySwTest_AddMapping
-- Purpose: Add a single cell mapping for a software test
-- Behavior: Silently succeeds if the mapping already exists (idempotent)
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

    -- Insert only if not already exists
    IF NOT EXISTS (SELECT 1 FROM floor.CellBySwTest
                   WHERE SwTestMapId = @SwTestMapId AND CellId = @CellId)
    BEGIN
        INSERT INTO floor.CellBySwTest (CellId, SwTestMapId)
        VALUES (@CellId, @SwTestMapId);
    END
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.CellBySwTest_DeleteMapping
-- Purpose: Remove a single cell mapping for a software test
-- Behavior: Silently succeeds if the mapping does not exist (idempotent)
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

    DELETE FROM floor.CellBySwTest
    WHERE SwTestMapId = @SwTestMapId AND CellId = @CellId;
END
GO

PRINT 'CellBySwTest operations created successfully.';
GO
