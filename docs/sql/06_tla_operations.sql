/*
    PE_Metrics Dimension Management - TLA (Part Number) Operations

    Objects created:
    - mgmt.vw_TLA           : View for listing all TLAs with IsUsed indicator
    - mgmt.TLA_GetByPartNo  : Get single TLA by PartNo
    - mgmt.TLA_Insert       : Insert new TLA
    - mgmt.TLA_Update       : Update existing TLA
    - mgmt.TLA_Delete       : Hard delete TLA (only if not used)

    Business Rules:
    - PartNo is the primary key (natural key from ERP)
    - TLAs can be hard deleted ONLY if not referenced in activity.ProductionTest
    - IsUsed flag indicates if PartNo exists in activity.ProductionTest
*/

USE PE_Metrics;
GO

--------------------------------------------------------------------------------
-- VIEW: mgmt.vw_TLA
-- Purpose: Read-only view of all TLAs with IsUsed indicator
-- IsUsed = 1 if PartNo exists in activity.ProductionTest
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
-- PROCEDURE: mgmt.TLA_GetByPartNo
-- Purpose: Retrieve a single TLA by its PartNo
-- Returns: Single row or empty if not found
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
    FROM product.TLA t
    WHERE t.PartNo = @PartNo;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.TLA_Insert
-- Purpose: Insert a new TLA
-- Errors:
--   50040 - PartNo already exists
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

    -- Validate PartNo uniqueness
    IF EXISTS (SELECT 1 FROM product.TLA WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50040, 16, 1, 'Part number already exists.');
        RETURN;
    END

    -- Insert the new TLA
    INSERT INTO product.TLA (PartNo, Family, Subfamily, ServiceGroup, FormalDescription, Description)
    VALUES (@PartNo, @Family, @Subfamily, @ServiceGroup, @FormalDescription, @Description);
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.TLA_Update
-- Purpose: Update an existing TLA
-- Note: PartNo cannot be changed (it's the primary key)
-- Errors:
--   50041 - TLA not found
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

    -- Validate TLA exists
    IF NOT EXISTS (SELECT 1 FROM product.TLA WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50041, 16, 1, 'Part number not found.');
        RETURN;
    END

    -- Update the TLA
    UPDATE product.TLA
    SET Family = @Family,
        Subfamily = @Subfamily,
        ServiceGroup = @ServiceGroup,
        FormalDescription = @FormalDescription,
        Description = @Description
    WHERE PartNo = @PartNo;
END
GO

--------------------------------------------------------------------------------
-- PROCEDURE: mgmt.TLA_Delete
-- Purpose: Hard delete a TLA (only if not used in production tests)
-- Errors:
--   50041 - TLA not found
--   50042 - Cannot delete: TLA is referenced in production tests
--   50043 - Cannot delete: TLA is referenced in cell mappings
--------------------------------------------------------------------------------
IF OBJECT_ID('mgmt.TLA_Delete', 'P') IS NOT NULL
    DROP PROCEDURE mgmt.TLA_Delete;
GO

CREATE PROCEDURE mgmt.TLA_Delete
    @PartNo VARCHAR(32)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate TLA exists
    IF NOT EXISTS (SELECT 1 FROM product.TLA WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50041, 16, 1, 'Part number not found.');
        RETURN;
    END

    -- Check if used in production tests
    IF EXISTS (SELECT 1 FROM activity.ProductionTest WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50042, 16, 1, 'Cannot delete: Part number is referenced in production tests.');
        RETURN;
    END

    -- Check if used in cell mappings (must delete those first or cascade)
    IF EXISTS (SELECT 1 FROM floor.CellByPartNo WHERE PartNo = @PartNo)
    BEGIN
        RAISERROR(50043, 16, 1, 'Cannot delete: Part number is referenced in cell mappings. Remove cell mappings first.');
        RETURN;
    END

    -- Delete the TLA
    DELETE FROM product.TLA
    WHERE PartNo = @PartNo;
END
GO

PRINT 'TLA operations created successfully.';
GO
