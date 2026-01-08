# Database Schema

This document describes the database schema for PE_Metrics dimension management.

- a star schema of dimensions and facts for manufacturing metrics around testing and calibration of water quality devices;
- this data model is intentionally not fully normalized as it is for OLAP, not OLTP;
- the primary fact is the pass or fail result of a test run against a product;
- from this, key metrics such as 'First Pass Yield' and 'Rolled Throughput Yield' can be calculated and tracked;


## Connection Information

<!-- Document connection string configuration, environment variables, or config files used -->

## Schemas

### floor

**Purpose**: schema for production floor reference and dimensions: cells, pc workstations in each cell

### sw

**Purpose**: schema for software dimension: applications that execute production tests and calibration

### product

**Purpose**: schema for products (final top level assembly) and product family

### activity

**Purpose**: schema for actual testing output and related activity: this holds the fact table

### dim

**Purpose**: schema for generic Power BI 'dimensions' such as a calendar

---

## Tables

### dim.Calendar

**Purpose**: a date table specifically for enabling Power BI time dimension capabilities; 

- for metrics and reporting purposes, the 'calendar day' of a test run is sufficient as the lowest time unit although we collect actual data to the microsecond level

**Definition**:
```sql
create table dim.Calendar
(
    CalendarDate   date       not null,
    DayOfWeek      tinyint    not null,
    DayOfMonth     tinyint    not null,
    DayOfYear      smallint   not null,
    DayName        varchar(9) not null,
    CalendarWeek   tinyint    not null,
    MonthOrdinal   tinyint    not null,
    MonthName      varchar(9) not null,
    FirstOfMonth   date       not null,
    LastOfMonth    date       not null,
    FiscalQuarter  tinyint    not null,
    FirstOfQuarter date       not null,
    LastOfQuarter  date       not null,
    CalendarYear   smallint   not null,
    IsLeapYear     bit        not null,
    FirstOfYear    date       not null,
    LastOfYear     date       not null,
    MMDDYYYY_US    char(10)   not null,
    YYYYMMDD       char(9)    not null,
    ISO8601        char(10)   not null
);

create unique clustered index PK_Calendar_CalData on dim.Calendar (CalendarDate);

create unique index UQ_Calendar_YYYYMMDD on dim.Calendar (YYYYMMDD);
```

### floor.Cell

**Purpose**: a cell is responsible for assemblying, calibrating, and testing a specific product or closely related set of products

**Definition**:
```sql
create table floor.Cell
(
    CellName         varchar(64)  not null constraint UQ_floor_Cell_Name unique,
    DisplayName      varchar(128) not null constraint UQ_floor_Cell_Display unique,
    ActiveFrom       date         not null,
    ActiveTo         date,
    Description      varchar(1024),
    AlternativeNames varchar(1024),
    CellId           int identity (1000, 1) constraint PK_floor_Cell primary key
)
```

### floor.PCStation

**Purpose**: since most tests record the network name of the pc they run on, it is the most reliable data point for mapping a test run to a cell

**Definition**:
```sql
create table floor.PCStation
(
    PcName varchar(128) not null primary key
)
```

### floor.CellByPCStation

**Purpose**: a pc workstation is allocated to one and only one cell for a given time period; 

- a pc can be obsolted, replaced, renamed, or re-assigned to another cell but always for a discrete time period usually in the range of months to years

**Definition**:
```sql
create table floor.CellByPCStation
(
    StationMapId int identity (1000, 1) constraint PK_CellByPCStation primary key,
    CellId       int          not null constraint FK_CellByPCStation_Cell references floor.Cell,
    PcName       varchar(128) not null constraint FK_CellByPCStation_PC references floor.PCStation,
    PcPurpose    varchar(256),
    ActiveFrom   date         not null,
    ActiveTo     date,
    ExtendedName varchar(256)
);

create unique index uq_floor_CellByPCStation on floor.CellByPCStation (CellId, PcName, ActiveFrom) include (ActiveTo);
```

### sw.SwTestMap

**Purpose**: each test or calibration software is named, has a unique numeric id, and is run for a specific product; 

- however, a test can be renamed, assigned a different id, or obsoleted; 
- 'ReportKey' is a unique string to which the same test is mapped across any names or id variations

**Definition**:
```sql
create table sw.SwTestMap
(
    SwTestMapId      int identity (1000, 1) constraint PK_sw_SwTestMap primary key,
    ConfiguredTestId varchar(16),
    TestApplication  varchar(128),
    TestName         varchar(256),
    ReportKey        varchar(128),
    TestDirectory    varchar(256),
    RelativePath     varchar(256),
    LastRun          date,
    Notes            varchar(256)
)
```

### floor.CellBySwTest

**Purpose**: a software test will be installed (on the workstations) in one or more cells

**Definition**:
```sql
create table floor.CellBySwTest
(
    CellId      int not null constraint FK_CellBySwTest_Cell references floor.Cell,
    SwTestMapId int not null constraint FK_CellBySwTest_SwTest references sw.SwTestMap,
    constraint PK_CellBySwTest primary key (CellId, SwTestMapId)
);

-- when cleaning the data, the recorded test id and name as recorded at time of test need to be mapped to the conformed and unchanging 'ReportKey'
create index IX_SwTestMap_RptKey on sw.SwTestMap (ConfiguredTestId, TestName, ReportKey);

```

### product.TLA

**Purpose**: tests are run against a product; 

- for metrics, we are interested in the part number corresponding to the final "top level assembly" (TLA); 
- this database is not for managing the products, which is the responsibility of the ERP system, so this table is highly denormalized.

**Definition**:
```sql
create table product.TLA
(
    PartNo            varchar(32) not null constraint PK_product_tla primary key,
    Family            varchar(64),
    Subfamily         varchar(96),
    ServiceGroup      varchar(32),
    FormalDescription varchar(256),
    Description       varchar(256)
)
```

### floor.CellByPartNo

**Purpose**: a part number is associated with one or more cells; 

- a report might look at the test runs for a part number broken down by cell;

**Definition**:
```sql
create table floor.CellByPartNo
(
    CellId int         not null constraint FK_CellByPart_Cell references floor.Cell,
    PartNo varchar(32) not null constraint FK_CellByPart_Part references product.TLA,
    constraint PK_CellByPart primary key (CellId, PartNo)
)
```

### activity.ProductionTest

**Purpose**: the primary 'fact' table representing the pass/fail result of a software test or calibration run on a specific workstation against a specific product in a specific cell; 

- each test run is specific to a unique serial number, and a serial number has many tests; 
- a serial number can have multiple unique software tests as well as multiple runs of the same test if it does not pass the first run;

**Definition**:
```sql
create table activity.ProductionTest
(
    SerialNo        bigint   not null,
    PartNo          varchar(32) constraint FK_ProductionTestYield_Product references product.TLA,
    SwTestMapId     int      not null constraint FK_ProductionTestYield_SwTest references sw.SwTestMap,
    CellId          int      not null constraint FK_ProductionTestYield_Cell references floor.Cell,
    StationMapId    int      not null constraint FK_ProductionTestYield_Station references floor.CellByPCStation,
    TestCalendarDay date     not null constraint FK_ProductionTestYield_Date references dim.Calendar (CalendarDate),
    TestTimeOfDay   time(0)  not null,
    TestDurationSec int      not null,
    OverallPass     bit      not null,
    TestRank        smallint not null,
    TestRuns        smallint not null,
    IsFirstPass     bit      not null,
    IsFinalPass     bit      not null,
    DeviceId        int,
    SensorId        int,
    StartTime       datetime not null,
    EndTime         datetime not null
);

-- a device under test, represented by a serial number, can only be tested by one application run on one workstation, in one cell, at a time
create unique clustered index UQ_ProductionTest_Key on activity.ProductionTest (SerialNo, SwTestMapId, StationMapId, StartTime);
```

---

## Views

### floor.CellByPCStation_Full

**Purpose**: a view representing the full attributes of each PCStation-to-Cell relation.

**Definition**:
```sql
CREATE VIEW floor.CellByPCStation_Full AS
		SELECT c.CellName,
		       c.DisplayName     AS CellDisplayName,
		       lookup.PcName,
		       lookup.PcPurpose,
		       lookup.ActiveFrom AS CellPCActiveFrom,
		       lookup.ActiveTo   AS CellPCActiveTo,
		       lookup.CellId,
		       lookup.StationMapId
		  FROM floor.CellByPCStation lookup
	INNER JOIN floor.Cell c ON lookup.CellId = c.CellId
```

### floor.CellByPartNo_Full

**Purpose**: a view representing the full attributes of each PartNumber-to-Cell relation

**Definition**:
```sql
CREATE VIEW floor.CellByPartNo_Full AS
			 SELECT c.CellName,
			        c.DisplayName AS CellDisplayName,
			        tla.PartNo,
			        tla.FormalDescription,
			        tla.Subfamily,
			        tla.Family,
			        c.CellId,
			        c.ActiveFrom AS CellActiveFrom,
			        c.ActiveTo   AS CellActiveTo
		       FROM floor.CellByPartNo lookup
	     INNER JOIN floor.Cell c on lookup.CellId = c.CellId
    LEFT OUTER JOIN product.TLA tla ON lookup.PartNo = tla.PartNo
```


### floor.CellBySwTest_Full

**Purpose**: a view representing the full attributes of each SoftwareTest-to-Cell relation

**Definition**:
```sql
CREATE VIEW floor.CellBySwTest_Full AS
			 SELECT c.CellName,
			        c.DisplayName AS CellDisplayName,
			        sw.ReportKey,
			        sw.ConfiguredTestId,
			        sw.TestApplication,
			        sw.TestName,
			        sw.TestDirectory,
			        sw.RelativePath,
			        sw.LastRun,
			        sw.Notes,
			        sw.SwTestMapId,
			        c.CellId,
			        c.ActiveFrom AS CellActiveFrom,
			        c.ActiveTo   AS CellActiveTo
			   FROM floor.CellBySwTest lookup
	     INNER JOIN floor.Cell c on lookup.CellId = c.CellId
	LEFT OUTER JOIN sw.SwTestMap sw ON lookup.SwTestMapId = sw.SwTestMapId
```

---

---

## Migration Notes

<!-- Document any migration scripts, version history, or upgrade procedures -->
