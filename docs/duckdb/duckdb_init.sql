-- DuckDB Cache Initialization Script
-- Creates tables matching SQL Server mgmt schema views
-- Tables use IF NOT EXISTS for idempotent execution

-- Cell table (maps to mgmt.vw_Cell)
CREATE TABLE IF NOT EXISTS Cell (
    CellId INTEGER NOT NULL PRIMARY KEY,
    CellName VARCHAR NOT NULL,
    DisplayName VARCHAR NOT NULL,
    ActiveFrom DATE NOT NULL,
    ActiveTo DATE,
    Description VARCHAR,
    AlternativeNames VARCHAR,
    IsActive INTEGER NOT NULL
);

-- PCStation table (maps to mgmt.vw_PCStation)
CREATE TABLE IF NOT EXISTS PCStation (
    PcName VARCHAR NOT NULL PRIMARY KEY
);

-- CellByPCStation table (maps to mgmt.vw_CellByPCStation)
-- Column order must match SQL Server view for SELECT * inserts:
-- StationMapId, CellId, CellName, PcName, PcPurpose, ActiveFrom, ActiveTo, ExtendedName, IsActive
CREATE TABLE IF NOT EXISTS CellByPCStation (
    StationMapId INTEGER NOT NULL PRIMARY KEY,
    CellId INTEGER NOT NULL,
    CellName VARCHAR,
    PcName VARCHAR NOT NULL,
    PcPurpose VARCHAR,
    ActiveFrom DATE NOT NULL,
    ActiveTo DATE,
    ExtendedName VARCHAR,
    IsActive INTEGER NOT NULL
);

-- SwTestMap table (maps to mgmt.vw_SwTestMap)
CREATE TABLE IF NOT EXISTS SwTestMap (
    SwTestMapId INTEGER NOT NULL PRIMARY KEY,
    ConfiguredTestId VARCHAR,
    TestApplication VARCHAR,
    TestName VARCHAR,
    ReportKey VARCHAR,
    TestDirectory VARCHAR,
    RelativePath VARCHAR,
    LastRun DATE,
    Notes VARCHAR,
    IsActive INTEGER NOT NULL
);

-- CellBySwTest table (maps to mgmt.vw_CellBySwTest)
-- Column order must match SQL Server view for SELECT * inserts:
-- SwTestMapId, ConfiguredTestId, TestApplication, ReportKey, LastRun, IsActive, CellId, CellName
CREATE TABLE IF NOT EXISTS CellBySwTest (
    SwTestMapId INTEGER NOT NULL,
    ConfiguredTestId VARCHAR,
    TestApplication VARCHAR,
    ReportKey VARCHAR,
    LastRun DATE,
    IsActive INTEGER NOT NULL,
    CellId INTEGER NOT NULL,
    CellName VARCHAR,
    PRIMARY KEY (SwTestMapId, CellId)
);

-- CellBySwTestView table (maps to mgmt.vw_CellBySwTest expanded view)
-- Same column order as CellBySwTest - both populated from same view
CREATE TABLE IF NOT EXISTS CellBySwTestView (
    SwTestMapId INTEGER NOT NULL,
    ConfiguredTestId VARCHAR,
    TestApplication VARCHAR,
    ReportKey VARCHAR,
    LastRun DATE,
    IsActive INTEGER NOT NULL,
    CellId INTEGER NOT NULL,
    CellName VARCHAR,
    PRIMARY KEY (SwTestMapId, CellId)
);

-- TLA table (maps to mgmt.vw_TLA)
CREATE TABLE IF NOT EXISTS TLA (
    PartNo VARCHAR NOT NULL PRIMARY KEY,
    Family VARCHAR,
    Subfamily VARCHAR,
    ServiceGroup VARCHAR,
    FormalDescription VARCHAR,
    Description VARCHAR,
    IsUsed INTEGER NOT NULL
);

-- CellByPartNo table (maps to mgmt.vw_CellByPartNo)
-- Column order must match SQL Server view for SELECT * inserts:
-- PartNo, Family, Subfamily, Description, CellId, CellName
CREATE TABLE IF NOT EXISTS CellByPartNo (
    PartNo VARCHAR NOT NULL,
    Family VARCHAR,
    Subfamily VARCHAR,
    Description VARCHAR,
    CellId INTEGER NOT NULL,
    CellName VARCHAR,
    PRIMARY KEY (PartNo, CellId)
);

-- CellByPartNoView table (maps to mgmt.vw_CellByPartNo expanded view)
-- Same column order as CellByPartNo - both populated from same view
CREATE TABLE IF NOT EXISTS CellByPartNoView (
    PartNo VARCHAR NOT NULL,
    Family VARCHAR,
    Subfamily VARCHAR,
    Description VARCHAR,
    CellId INTEGER NOT NULL,
    CellName VARCHAR,
    PRIMARY KEY (PartNo, CellId)
);
