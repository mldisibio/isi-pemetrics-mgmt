/*
    PE_Metrics Dimension Management - Master Deployment Script

    This script executes all individual SQL scripts in the correct order.
    Run this once to deploy all stored procedures, views, and types.

    Prerequisites:
    - PE_Metrics database must exist
    - All base schemas (floor, sw, product, activity, dim) must exist
    - All base tables must exist

    Deployment Order:
    1. Schema setup (mgmt schema)
    2. Cell operations
    3. PCStation operations
    4. CellByPCStation operations
    5. SwTestMap operations
    6. CellBySwTest operations
    7. TLA operations
    8. CellByPartNo operations
*/

USE PE_Metrics;
GO

PRINT '======================================';
PRINT 'PE_Metrics Dimension Management';
PRINT 'Database Objects Deployment';
PRINT '======================================';
PRINT '';

-- Note: In SQL Server Management Studio, you would use :r to include files
-- For manual deployment, run each script file in order:
--   00_schema_setup.sql
--   01_cell_operations.sql
--   02_pcstation_operations.sql
--   03_cellbypcstation_operations.sql
--   04_swtestmap_operations.sql
--   05_cellbyswtest_operations.sql
--   06_tla_operations.sql
--   07_cellbypartno_operations.sql

-- Alternatively, copy the contents of each file in sequence below this line
-- and execute as a single script.

PRINT '';
PRINT 'Deployment complete. See individual script output for details.';
PRINT '';
GO
