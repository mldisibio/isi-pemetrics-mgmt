# Project Progress

## Overview
PE_Metrics Dimension Management application - A .NET 9.0 Windows Forms application for CRUD operations on star schema dimension tables.

## Phase 1: Database Layer

**Status: COMPLETE - APPROVED AND DEPLOYED**

All stored procedures, views, and functions created in `mgmt` schema and deployed to `.\MLD2019` SQL Server.

### Deliverables
- [x] Schema setup (mgmt schema)
- [x] Cell operations (view, GetById with IsActive, Insert, Update)
- [x] PCStation operations (view, Search, idempotent Insert)
- [x] CellByPCStation operations (view, GetById with IsActive, Insert, Update)
- [x] SwTestMap operations (view with IsActive, GetById with IsActive, Insert, Update)
- [x] CellBySwTest operations (view, GetBySwTestMapId, SetMappings, Add/DeleteMapping with validation)
- [x] TLA operations (view with IsUsed, GetByPartNo, Insert, Update, idempotent Delete)
- [x] CellByPartNo operations (view, GetByPartNo, SetMappings, Add/DeleteMapping with validation)
- [x] Table-valued parameter type (mgmt.IntList)
- [x] Error codes documentation

---

## Phase 2: Data API Layer

**Status: COMPLETE - APPROVED**

### Architectural Restructuring (2026-01-13)

**Hexagonal Architecture:**
- [x] Move SQL Server repositories to separate adapter project (`PEMetrics.ProductionStore`)
- [x] Create empty cache adapter project (`PEMetrics.DataCache`)
- [x] Move `RepositoryException` to `Exceptions/` folder in core
- [x] Core contains only models, ports, mapping infrastructure, and exceptions
- [x] Adapters depend on core; core has zero dependencies on adapters

**CQRS Pattern Refactoring:**
- [x] Create unified query interface `ForReadingPEMetricsDimensions` (14 read operations)
- [x] Create `PEMetricsQueryRepository` implementing all reads from SQL Server
- [x] Refactor command interfaces to write-only operations (Insert, Update, Delete, SetMappings)
- [x] Update all command repositories to remove read operations
- [x] Consolidate 7 mapper interfaces into single `ForMappingDataModels`
- [x] Update `DataModelMappers` to implement unified mapper interface
- [x] Update all repositories to use `ForMappingDataModels`

### Project Structure
- [x] Create `src/core/PEMetrics.DataApi` class library
- [x] Create `src/adapters/PEMetrics.ProductionStore` adapter
- [x] Create `src/adapters/PEMetrics.DataCache` adapter (empty)
- [x] Add NuGet packages (Microsoft.Data.SqlClient, Configuration.Abstractions, Collections.Immutable)

### Domain Models
- [x] All models as immutable sealed records
- [x] View models for expanded grid data (`CellBySwTestView`, `CellByPartNoView`)
- [x] IsActive/IsUsed computed properties

### Ports/Interfaces

**Query Port (Read-only):**
- [x] `ForReadingPEMetricsDimensions` - Unified interface with 14 read operations

**Command Ports (Write-only):**
- [x] `ForManagingCells` - Insert, Update
- [x] `ForManagingPCStations` - Insert
- [x] `ForMappingPCStationToCell` - Insert, Update
- [x] `ForManagingSwTests` - Insert, Update
- [x] `ForMappingSwTestsToCells` - SetMappings, AddMapping, DeleteMapping
- [x] `ForManagingPartNumbers` - Insert, Update, Delete
- [x] `ForMappingPartNumberToCells` - SetMappings, AddMapping, DeleteMapping

### Mapping Infrastructure
- [x] `ForMappingDataModels` - Unified mapper interface
- [x] `DataModelMappers` - Single implementation for all models
- [x] `DataReaderExtensions` - MapAll, MapFirstOrDefault, nullable helpers

### Repository Implementations

**Query Repository:**
- [x] `PEMetricsQueryRepository` - Full implementation of all read operations

**Command Repositories:**
- [x] `CellRepository` - Full implementation
- [x] `PCStationRepository` - Full implementation
- [x] `CellByPCStationRepository` - Full implementation
- [x] `SwTestMapRepository` - Full implementation
- [x] `CellBySwTestRepository` - Full implementation (TVP support)
- [x] `TLARepository` - Full implementation
- [x] `CellByPartNoRepository` - Full implementation (TVP support)

### Infrastructure
- [x] `ForCreatingSqlServerConnections` - Connection factory interface (returns `DbConnection`)
- [x] `SqlConnectionFactory` - Implements interface using IConfiguration
- [x] `RepositoryException` - Business rule violations
- [x] `SqlErrorTranslator` - Error code to message translation

---

## Phase 3: Caching Layer

**Status: COMPLETE - AWAITING TESTING**

### Architecture Refinements (2026-01-15)
- [x] Move `ForCreatingDuckDbConnections` from DataCache adapter to core DataApi
- [x] Update connection interfaces to return `System.Data.Common.DbConnection` abstraction
- [x] Fix namespaces in ProductionStore repositories (PEMetrics.ProductionStore)
- [x] Fix CS8604 null reference warning in DuckDbInitializer

### Architecture & Design (Finalized 2026-01-13)

**DuckDB Cache Strategy:**
- [x] Use DuckDB.NET.Data.Full package
- [x] Use nanodbc community extension for SQL Server connectivity
- [x] Cache implements `ForReadingPEMetricsDimensions` (same as SQL Server)
- [x] Constructor injection: accepts SQL Server query repository for pass-through
- [x] Full table refresh strategy (datasets are small)

**Communication Method:**
- [x] Use nanodbc `odbc_scan` to read from SQL Server views
- [x] Pure SQL operations: `INSERT INTO duckdb_table SELECT * FROM odbc_scan(...)`
- [x] No ADO.NET reader loops or appenders
- [x] Fail with clear error if nanodbc unavailable (no fallback)

### Configuration (Approved)

**appsettings.json structure:**
```json
{
    "CacheConfiguration": {
        "CacheDbType": "DuckDb",
        "CachePath": "MyDocuments\\PEDimMgmnt\\PE_Metrics_Cache.duckdb",
        "DeleteOnExit": false,
        "InitSqlPath": "MyDocuments\\PEDimMgmnt\\duckdb_init.sql"
    },
    "ConnectionStrings": {
        "PEMetricsConnection": "Server=.\\MLD2019;...",
        "PEMetricsODBC": "Driver={SQL Server};Server=.\\MLD2019;Database=PE_Metrics;Trusted_Connection=yes;"
    }
}
```

**Path Resolution Rules:**
- [x] `MyDocuments` prefix → `Environment.SpecialFolder.Personal`
- [x] Absolute paths → use as-is
- [x] Relative paths → relative to executable

### Infrastructure Components

**Configuration:**
- [x] `CacheConfiguration` - POCO for appsettings binding
- [x] `CachePathResolver` - Resolves MyDocuments, absolute, relative paths

**DuckDB Setup:**
- [x] `DuckDbConnectionFactory` - Implements `ForCreatingDuckDbConnections` (returns `DbConnection`)
- [x] `DuckDbInitializer` - Startup init, execute init.sql, cleanup on exit
- [x] Table creation in `duckdb_init.sql` (IF NOT EXISTS)

**Query Repository:**
- [x] `DuckDbQueryRepository` - Implements `ForReadingPEMetricsDimensions`
- [x] Uses same `ForMappingDataModels` as SQL Server
- [x] Queries local DuckDB tables

### Notification System

**New Ports in Core:**
- [x] `ForNotifyingDataChanges` - Publish data change events
  - Methods: NotifyCellChanged, NotifyPCStationChanged, etc.
- [x] `ForNotifyingDataCommunicationErrors` - Handle connectivity errors
  - Methods: ProductionStoreNotReachable, UnexpectedError

**Event Flow:**
- [x] Command repositories inject `ForNotifyingDataChanges`
- [x] Call notification methods after successful writes
- [x] Cache subscribes and queues refresh requests

### Cache Refresh Service

**Async Processing:**
- [x] Use `System.Threading.Channels.Channel<RefreshRequest>`
- [x] Producer/consumer pattern
- [x] Background service processes queue until application exit

**Startup Population:**
- [x] Populate all tables asynchronously on startup
- [x] Max 4 tables in parallel (configurable)
- [x] Use `SemaphoreSlim` per table
- [x] Queries block if table population in progress

**Refresh Dependencies:**
```
Cell update → Cell, CellByPCStation, CellBySwTest, CellBySwTestView, CellByPartNo, CellByPartNoView
SwTestMap update → SwTestMap, CellBySwTest, CellBySwTestView
TLA update → TLA, CellByPartNo, CellByPartNoView
Other operations → single table refresh
```

### Error Handling

**Startup Health Check:**
- [x] Test SQL Server connectivity once at startup
- [x] Timeout/network error → `ProductionStoreNotReachable`, enter offline mode
- [x] No exception thrown - graceful degradation
- [x] Offline mode: reads from cache, writes disabled in UI

**Runtime Error Strategy:**
- [x] All errors after startup → `UnexpectedError`
- [x] Query operations → return empty collections
- [x] Command operations → return -1 or false (functional style)
- [x] No exception throwing - notification IS the handling
- [x] Required dependency (non-nullable), use no-op implementation if not needed

**SQL Exception Detection:**
- [x] Timeout/network errors: SqlException numbers -1, -2, 2, 53
- [x] All other SqlExceptions → `UnexpectedError`

### Implementation Tasks

**Configuration & Infrastructure:**
- [x] Create `CacheConfiguration` class
- [x] Create `CachePathResolver` with MyDocuments support
- [x] Create `DuckDbConnectionFactory`
- [x] Create `DuckDbInitializer` (nanodbc, schema, cleanup)

**Query Repository:**
- [x] Create `DuckDbQueryRepository`
- [x] Implement all 14 read operations
- [x] Add semaphore blocking for table population

**Notification System:**
- [x] Create `ForNotifyingDataChanges` interface in core
- [x] Create `ForNotifyingDataCommunicationErrors` interface in core
- [x] Create `NoOpErrorNotifier` implementation
- [x] Create `NoOpDataChangeNotifier` implementation
- [x] Create `DataChangeNotificationHandler` (cache subscriber)
- [x] Create `CacheRefreshService` (Channel consumer)

**Health Check & Startup:**
- [x] Create `ProductionStoreHealthCheck`
- [x] Implement connectivity test
- [ ] Wire into application startup (Phase 4)
- [x] Implement parallel table population

**Repository Updates:**
- [x] Inject `ForNotifyingDataCommunicationErrors` into all repositories
- [x] Inject `ForNotifyingDataChanges` into command repositories
- [x] Wrap operations with try-catch error handling
- [x] Call notification methods after successful writes
- [x] Update return types (query → empty, command → -1/false on error)

**Testing:**
- [ ] Test online mode with successful SQL Server connection
- [ ] Test offline mode with unreachable SQL Server
- [ ] Test cache population on startup
- [ ] Test cache refresh on data changes
- [ ] Test error handling and notifications
- [ ] Verify nanodbc extension works

---

## Integration Tests (Phase 2/3 Validation)

**Status: COMPLETE**

### Test Project Structure
- [x] Created `tests/PEMetrics.IntegrationTests/` with xUnit framework
- [x] Testcontainers.MsSql for SQL Server container management
- [x] SQL scripts for database initialization (schemas, tables, procedures, seed data)
- [x] Test fixtures with shared container via ICollectionFixture

### Test Coverage (75 tests total)
- [x] **Query Repository Tests** (14 tests)
  - All 14 read operations verified to return data
  - GetById methods work correctly
  - Search methods work correctly
  - Computed flags (IsActive, IsUsed) verified
- [x] **CellRepository Tests** (8 tests) - Insert, Update, notifications, error handling
- [x] **PCStationRepository Tests** (4 tests) - Idempotent Insert, notifications
- [x] **CellByPCStationRepository Tests** (5 tests) - Insert, Update, notifications
- [x] **SwTestMapRepository Tests** (6 tests) - Insert, Update, duplicate handling
- [x] **CellBySwTestRepository Tests** (10 tests) - SetMappings, AddMapping, DeleteMapping
- [x] **TLARepository Tests** (10 tests) - Insert, Update, Delete with referential integrity
- [x] **CellByPartNoRepository Tests** (10 tests) - SetMappings, AddMapping, DeleteMapping

### Test Infrastructure
- [x] `SqlServerContainerFixture` - Shared container with SQL script execution
- [x] `TestConnectionFactory` - Implements `ForCreatingSqlServerConnections`
- [x] `RecordingNotifier` - Verifies `ForNotifyingDataChanges` contract
- [x] `RecordingErrorNotifier` - Verifies `ForNotifyingDataCommunicationErrors` contract
- [x] SQL Scripts: 01_CreateSchemas, 02_CreateBaseTables, 03_CreateMgmtObjects, 04_SeedData

---

## Phase 4: Windows Forms UI Layer

**Status: NOT STARTED**

### Project Setup
- [ ] Update existing `DimensionManagement` project
- [ ] Add references to DataApi and adapter projects
- [ ] Configure dependency injection
- [ ] Wire up error notifications (offline mode indicator)
- [ ] Create main form with navigation

### Screen 1: Cell Management
- [ ] Cell list DataGridView
- [ ] Active Only filter
- [ ] Add/Edit dialog
- [ ] Integration with cache and data API

### Screen 2: PC Station Management
- [ ] PC name input with autocomplete
- [ ] Add confirmation dialog

### Screen 3: PC to Cell Mapping Management
- [ ] Mapping list DataGridView
- [ ] Active Only filter
- [ ] Add/Edit dialog with Cell picker

### Screen 4: Software Test Management
- [ ] SwTestMap list DataGridView
- [ ] Active filter
- [ ] Add/Edit dialog with Cell checkbox list

### Screen 5: Part Number (TLA) Management
- [ ] TLA list DataGridView with IsUsed column
- [ ] Text search filter
- [ ] IsUsed filter
- [ ] Add/Edit dialog with Cell checkbox list
- [ ] Delete button (disabled when IsUsed)

---

## Design Decisions & Patterns

### Phase 1 (Database)
- Views for read-only data (not TVFs)
- Computed IsActive/IsUsed flags in views AND GetById procedures
- Table-valued parameters for bulk operations (mgmt.IntList)
- Atomic SetMappings with delete-then-insert in transactions
- Error codes organized by entity (50001-50009, etc.)
- Idempotent operations where appropriate (Insert, Delete, Add/DeleteMapping)

### Phase 2 (Data API)
- **Hexagonal Architecture**: Core defines ports, adapters implement them
- **CQRS Pattern**: Separate query (ForReadingPEMetricsDimensions) from commands
- **Mapper Pattern**: Single ForMappingDataModels shared across adapters
- **Interface Naming**: Cockburn/Henney style (ForDoingSomething)
- **Immutability**: Sealed records with init properties, ImmutableList<T> returns
- **Functional Style**: Methods return values (-1/false for errors, not void)

### Phase 3 (Cache - Implemented)
- **DuckDB with nanodbc**: Pure SQL operations via `odbc_scan`, no ADO.NET loops
- **CQRS Query Implementation**: DuckDB implements same ForReadingPEMetricsDimensions
- **Connection Abstraction**: Core defines ports with `System.Data.Common.DbConnection` return type
- **Notification Pattern**: Publisher (commands) / Subscriber (cache) via ports
- **Async Refresh**: Channel-based producer/consumer pattern with `RefreshRequest`
- **Error Handling**: Notifications instead of exceptions, graceful degradation
- **Offline Mode**: Startup health check, cache serves stale data, writes disabled
- **Table Population**: Parallel population with semaphore blocking per table

---

## Testing Notes
- SQL scripts deployed and tested on `.\MLD2019`
- Build succeeds with zero warnings
- All repositories compile and follow established patterns
- Phase 3 caching layer implemented and compiles successfully
- Architecture refined with `DbConnection` abstraction in core ports
- **Integration tests complete**: 75 tests passing using Testcontainers for SQL Server
- DuckDB cache layer requires manual testing with actual nanodbc extension
