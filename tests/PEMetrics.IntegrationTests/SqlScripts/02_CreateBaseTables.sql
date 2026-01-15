-- Create base tables for PE_Metrics testing
-- Tables are created in dependency order

-- dim.Calendar (no dependencies)
CREATE TABLE dim.Calendar
(
    CalendarDate   DATE       NOT NULL,
    DayOfWeek      TINYINT    NOT NULL,
    DayOfMonth     TINYINT    NOT NULL,
    DayOfYear      SMALLINT   NOT NULL,
    DayName        VARCHAR(9) NOT NULL,
    CalendarWeek   TINYINT    NOT NULL,
    MonthOrdinal   TINYINT    NOT NULL,
    MonthName      VARCHAR(9) NOT NULL,
    FirstOfMonth   DATE       NOT NULL,
    LastOfMonth    DATE       NOT NULL,
    FiscalQuarter  TINYINT    NOT NULL,
    FirstOfQuarter DATE       NOT NULL,
    LastOfQuarter  DATE       NOT NULL,
    CalendarYear   SMALLINT   NOT NULL,
    IsLeapYear     BIT        NOT NULL,
    FirstOfYear    DATE       NOT NULL,
    LastOfYear     DATE       NOT NULL,
    MMDDYYYY_US    CHAR(10)   NOT NULL,
    YYYYMMDD       CHAR(9)    NOT NULL,
    ISO8601        CHAR(10)   NOT NULL
);
GO

CREATE UNIQUE CLUSTERED INDEX PK_Calendar_CalData ON dim.Calendar (CalendarDate);
GO

-- floor.Cell (no dependencies)
CREATE TABLE floor.Cell
(
    CellName         VARCHAR(64)  NOT NULL CONSTRAINT UQ_floor_Cell_Name UNIQUE,
    DisplayName      VARCHAR(128) NOT NULL CONSTRAINT UQ_floor_Cell_Display UNIQUE,
    ActiveFrom       DATE         NOT NULL,
    ActiveTo         DATE,
    Description      VARCHAR(1024),
    AlternativeNames VARCHAR(1024),
    CellId           INT IDENTITY (1000, 1) CONSTRAINT PK_floor_Cell PRIMARY KEY
);
GO

-- floor.PCStation (no dependencies)
CREATE TABLE floor.PCStation
(
    PcName VARCHAR(128) NOT NULL PRIMARY KEY
);
GO

-- floor.CellByPCStation (depends on Cell, PCStation)
CREATE TABLE floor.CellByPCStation
(
    StationMapId INT IDENTITY (1000, 1) CONSTRAINT PK_CellByPCStation PRIMARY KEY,
    CellId       INT          NOT NULL CONSTRAINT FK_CellByPCStation_Cell REFERENCES floor.Cell,
    PcName       VARCHAR(128) NOT NULL CONSTRAINT FK_CellByPCStation_PC REFERENCES floor.PCStation,
    PcPurpose    VARCHAR(256),
    ActiveFrom   DATE         NOT NULL,
    ActiveTo     DATE,
    ExtendedName VARCHAR(256)
);
GO

CREATE UNIQUE INDEX uq_floor_CellByPCStation ON floor.CellByPCStation (CellId, PcName, ActiveFrom) INCLUDE (ActiveTo);
GO

-- sw.SwTestMap (no dependencies)
CREATE TABLE sw.SwTestMap
(
    SwTestMapId      INT IDENTITY (1000, 1) CONSTRAINT PK_sw_SwTestMap PRIMARY KEY,
    ConfiguredTestId VARCHAR(16),
    TestApplication  VARCHAR(128),
    TestName         VARCHAR(256),
    ReportKey        VARCHAR(128),
    TestDirectory    VARCHAR(256),
    RelativePath     VARCHAR(256),
    LastRun          DATE,
    Notes            VARCHAR(256)
);
GO

CREATE INDEX UQ_SwTestMap_IdName ON sw.SwTestMap (ConfiguredTestId, TestName);
GO

-- floor.CellBySwTest (depends on Cell, SwTestMap)
CREATE TABLE floor.CellBySwTest
(
    CellId      INT NOT NULL CONSTRAINT FK_CellBySwTest_Cell REFERENCES floor.Cell,
    SwTestMapId INT NOT NULL CONSTRAINT FK_CellBySwTest_SwTest REFERENCES sw.SwTestMap,
    CONSTRAINT PK_CellBySwTest PRIMARY KEY (CellId, SwTestMapId)
);
GO

-- product.TLA (no dependencies)
CREATE TABLE product.TLA
(
    PartNo            VARCHAR(32) NOT NULL CONSTRAINT PK_product_tla PRIMARY KEY,
    Family            VARCHAR(64),
    Subfamily         VARCHAR(96),
    ServiceGroup      VARCHAR(32),
    FormalDescription VARCHAR(256),
    Description       VARCHAR(256)
);
GO

-- floor.CellByPartNo (depends on Cell, TLA)
CREATE TABLE floor.CellByPartNo
(
    CellId INT         NOT NULL CONSTRAINT FK_CellByPart_Cell REFERENCES floor.Cell,
    PartNo VARCHAR(32) NOT NULL CONSTRAINT FK_CellByPart_Part REFERENCES product.TLA,
    CONSTRAINT PK_CellByPart PRIMARY KEY (CellId, PartNo)
);
GO

-- activity.ProductionTest (fact table - depends on Calendar, Cell, CellByPCStation, SwTestMap, TLA)
CREATE TABLE activity.ProductionTest
(
    SerialNo        BIGINT      NOT NULL,
    PartNo          VARCHAR(32) CONSTRAINT FK_ProductionTestYield_Product REFERENCES product.TLA,
    SwTestMapId     INT         NOT NULL CONSTRAINT FK_ProductionTestYield_SwTest REFERENCES sw.SwTestMap,
    CellId          INT         NOT NULL CONSTRAINT FK_ProductionTestYield_Cell REFERENCES floor.Cell,
    StationMapId    INT         NOT NULL CONSTRAINT FK_ProductionTestYield_Station REFERENCES floor.CellByPCStation,
    TestCalendarDay DATE        NOT NULL CONSTRAINT FK_ProductionTestYield_Date REFERENCES dim.Calendar (CalendarDate),
    TestTimeOfDay   TIME(0)     NOT NULL,
    TestDurationSec INT         NOT NULL,
    OverallPass     BIT         NOT NULL,
    TestRank        SMALLINT    NOT NULL,
    TestRuns        SMALLINT    NOT NULL,
    IsFirstPass     BIT         NOT NULL,
    IsFinalPass     BIT         NOT NULL,
    DeviceId        INT,
    SensorId        INT,
    StartTime       DATETIME    NOT NULL,
    EndTime         DATETIME    NOT NULL
);
GO

CREATE UNIQUE CLUSTERED INDEX UQ_ProductionTest_Key
    ON activity.ProductionTest (SerialNo, SwTestMapId, StationMapId, StartTime);
GO
