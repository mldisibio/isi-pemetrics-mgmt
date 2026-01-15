-- Seed data for PE_Metrics integration tests
-- Provides minimal test data for all views and repositories

-- Calendar data (minimal - just test dates)
INSERT INTO dim.Calendar (CalendarDate, DayOfWeek, DayOfMonth, DayOfYear, DayName, CalendarWeek,
    MonthOrdinal, MonthName, FirstOfMonth, LastOfMonth, FiscalQuarter, FirstOfQuarter,
    LastOfQuarter, CalendarYear, IsLeapYear, FirstOfYear, LastOfYear, MMDDYYYY_US, YYYYMMDD, ISO8601)
VALUES
    ('2024-01-01', 2, 1, 1, 'Monday', 1, 1, 'January', '2024-01-01', '2024-01-31', 1, '2024-01-01', '2024-03-31', 2024, 1, '2024-01-01', '2024-12-31', '01/01/2024', '20240101', '2024-01-01'),
    ('2024-06-15', 7, 15, 167, 'Saturday', 24, 6, 'June', '2024-06-01', '2024-06-30', 2, '2024-04-01', '2024-06-30', 2024, 1, '2024-01-01', '2024-12-31', '06/15/2024', '20240615', '2024-06-15'),
    ('2024-12-31', 3, 31, 366, 'Tuesday', 53, 12, 'December', '2024-12-01', '2024-12-31', 4, '2024-10-01', '2024-12-31', 2024, 1, '2024-01-01', '2024-12-31', '12/31/2024', '20241231', '2024-12-31');
GO

-- Cells (3 test cells with different active states)
SET IDENTITY_INSERT floor.Cell ON;
INSERT INTO floor.Cell (CellId, CellName, DisplayName, ActiveFrom, ActiveTo, Description, AlternativeNames)
VALUES
    (1001, 'TestCellA', 'Test Cell Alpha', '2020-01-01', NULL, 'Active test cell A', 'CellA,Alpha'),
    (1002, 'TestCellB', 'Test Cell Beta', '2020-01-01', NULL, 'Active test cell B', NULL),
    (1003, 'TestCellC', 'Test Cell Gamma', '2020-01-01', '2023-12-31', 'Inactive test cell C', 'CellC');
SET IDENTITY_INSERT floor.Cell OFF;
GO

-- PC Stations (4 test stations)
INSERT INTO floor.PCStation (PcName)
VALUES ('PC-ALPHA-01'), ('PC-ALPHA-02'), ('PC-BETA-01'), ('PC-UNUSED-01');
GO

-- CellByPCStation mappings (3 active mappings)
SET IDENTITY_INSERT floor.CellByPCStation ON;
INSERT INTO floor.CellByPCStation (StationMapId, CellId, PcName, PcPurpose, ActiveFrom, ActiveTo, ExtendedName)
VALUES
    (1001, 1001, 'PC-ALPHA-01', 'Main test station', '2020-01-01', NULL, NULL),
    (1002, 1001, 'PC-ALPHA-02', 'Backup station', '2020-01-01', NULL, NULL),
    (1003, 1002, 'PC-BETA-01', 'Beta cell station', '2020-06-01', NULL, NULL);
SET IDENTITY_INSERT floor.CellByPCStation OFF;
GO

-- SwTestMap entries (4 test mappings with different IsActive states)
SET IDENTITY_INSERT sw.SwTestMap ON;
INSERT INTO sw.SwTestMap (SwTestMapId, ConfiguredTestId, TestApplication, TestName, ReportKey, TestDirectory, RelativePath, LastRun, Notes)
VALUES
    (1001, 'T001', 'CalibrationApp', 'Sensor Calibration Test', 'SENSOR_CAL', 'C:\Tests\Calibration', 'SensorCal', '2024-06-15', 'Primary calibration'),
    (1002, 'T002', 'ValidationApp', 'Final Validation Test', 'FINAL_VAL', 'C:\Tests\Validation', 'FinalVal', '2024-06-15', NULL),
    (1003, 'T003', 'LegacyApp', 'Legacy Test Obsolete', 'LEGACY', NULL, NULL, '2020-01-01', 'No longer used'),
    (1004, 'T004', 'CalibrationApp', 'pH Calibration', 'PH_CAL', 'C:\Tests\Calibration', 'pHCal', NULL, 'New test not yet run');
SET IDENTITY_INSERT sw.SwTestMap OFF;
GO

-- CellBySwTest mappings
INSERT INTO floor.CellBySwTest (CellId, SwTestMapId)
VALUES
    (1001, 1001), (1001, 1002),  -- Cell A has 2 tests
    (1002, 1001), (1002, 1002);  -- Cell B has same 2 tests
GO

-- TLA entries (4 part numbers)
INSERT INTO product.TLA (PartNo, Family, Subfamily, ServiceGroup, FormalDescription, Description)
VALUES
    ('PN-001-A', 'Sensors', 'pH Sensors', 'SG-PH', 'pH Sensor Model A', 'Standard pH sensor'),
    ('PN-001-B', 'Sensors', 'pH Sensors', 'SG-PH', 'pH Sensor Model B', 'Premium pH sensor'),
    ('PN-002-A', 'Controllers', 'Basic Controllers', 'SG-CTRL', 'Basic Controller', 'Entry level controller'),
    ('PN-UNUSED', 'Testing', NULL, NULL, 'Test Part - Unused', 'Can be deleted');
GO

-- CellByPartNo mappings
INSERT INTO floor.CellByPartNo (CellId, PartNo)
VALUES
    (1001, 'PN-001-A'), (1001, 'PN-001-B'),  -- Cell A has 2 parts
    (1002, 'PN-001-A'), (1002, 'PN-002-A');  -- Cell B has 2 parts
GO

-- One ProductionTest record to mark PN-001-A as "IsUsed"
INSERT INTO activity.ProductionTest (SerialNo, PartNo, SwTestMapId, CellId, StationMapId, TestCalendarDay,
    TestTimeOfDay, TestDurationSec, OverallPass, TestRank, TestRuns, IsFirstPass, IsFinalPass, StartTime, EndTime)
VALUES
    (1000000001, 'PN-001-A', 1001, 1001, 1001, '2024-06-15', '10:00:00', 120, 1, 1, 1, 1, 1, '2024-06-15 10:00:00', '2024-06-15 10:02:00');
GO
