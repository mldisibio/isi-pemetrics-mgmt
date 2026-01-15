-- Create all schemas required for PE_Metrics testing
-- Note: 'mgmt' schema is created by the mgmt objects script

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'floor')
    EXEC('CREATE SCHEMA floor');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'sw')
    EXEC('CREATE SCHEMA sw');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'product')
    EXEC('CREATE SCHEMA product');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'activity')
    EXEC('CREATE SCHEMA activity');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dim')
    EXEC('CREATE SCHEMA dim');
GO
