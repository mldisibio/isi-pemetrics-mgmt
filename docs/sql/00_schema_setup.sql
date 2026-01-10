/*
    PE_Metrics Dimension Management - Schema Setup

    This script creates the 'mgmt' schema for all stored procedures, views,
    and functions used by the Dimension Management application.

    Run this script first before any other management scripts.
*/

USE PE_Metrics;
GO

-- Create the mgmt schema if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'mgmt')
BEGIN
    EXEC('CREATE SCHEMA mgmt');
END
GO

PRINT 'Schema [mgmt] is ready.';
GO
