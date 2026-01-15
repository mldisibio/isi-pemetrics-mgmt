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
CREATE TABLE IF NOT EXISTS CellByPCStation (
    StationMapId INTEGER NOT NULL PRIMARY KEY,
    CellId INTEGER NOT NULL,
    PcName VARCHAR NOT NULL,
    PcPurpose VARCHAR,
    ActiveFrom DATE NOT NULL,
    ActiveTo DATE,
    ExtendedName VARCHAR,
    CellName VARCHAR,
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

-- CellBySwTest table (maps to mgmt.CellBySwTest_GetBySwTestMapId result)
-- No view for this, only used for specific lookups
CREATE TABLE IF NOT EXISTS CellBySwTest (
    SwTestMapId INTEGER NOT NULL,
    CellId INTEGER NOT NULL,
    CellName VARCHAR,
    PRIMARY KEY (SwTestMapId, CellId)
);

-- CellBySwTestView table (maps to mgmt.vw_CellBySwTest expanded view)
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

-- CellByPartNo table (maps to mgmt.CellByPartNo_GetByPartNo result)
-- No view for this, only used for specific lookups
CREATE TABLE IF NOT EXISTS CellByPartNo (
    PartNo VARCHAR NOT NULL,
    CellId INTEGER NOT NULL,
    CellName VARCHAR,
    PRIMARY KEY (PartNo, CellId)
);

-- CellByPartNoView table (maps to mgmt.vw_CellByPartNo expanded view)
CREATE TABLE IF NOT EXISTS CellByPartNoView (
    PartNo VARCHAR NOT NULL,
    Family VARCHAR,
    Subfamily VARCHAR,
    Description VARCHAR,
    CellId INTEGER NOT NULL,
    CellName VARCHAR,
    PRIMARY KEY (PartNo, CellId)
);
