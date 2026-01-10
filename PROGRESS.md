# Project Progress

## Overview
PE_Metrics Dimension Management application - A .NET 9.0 Windows Forms application for CRUD operations on star schema dimension tables.

## Phase 1: Database Layer
All stored procedures, views, and functions are created in the `mgmt` schema.

**Status: COMPLETE - AWAITING REVIEW**

### Schema Setup
- [x] Create `mgmt` schema if not exists (`00_schema_setup.sql`)

### floor.Cell Table Operations (`01_cell_operations.sql`)
- [x] `mgmt.vw_Cell` - View for listing all cells with IsActive flag
- [x] `mgmt.Cell_GetById` - Get single cell by ID
- [x] `mgmt.Cell_Insert` - Insert new cell, return identity via OUTPUT
- [x] `mgmt.Cell_Update` - Update existing cell

### floor.PCStation Table Operations (`02_pcstation_operations.sql`)
- [x] `mgmt.vw_PCStation` - View for listing all PC stations
- [x] `mgmt.PCStation_Search` - Search PC stations by prefix (for autocomplete)
- [x] `mgmt.PCStation_Exists` - Check if PC name exists (OUTPUT bit)
- [x] `mgmt.PCStation_Insert` - Insert new PC station

### floor.CellByPCStation Table Operations (`03_cellbypcstation_operations.sql`)
- [x] `mgmt.vw_CellByPCStation` - View for listing with Cell names and IsActive flag
- [x] `mgmt.CellByPCStation_GetById` - Get single mapping by ID
- [x] `mgmt.CellByPCStation_Insert` - Insert new mapping, return identity via OUTPUT
- [x] `mgmt.CellByPCStation_Update` - Update existing mapping

### sw.SwTestMap Table Operations (`04_swtestmap_operations.sql`)
- [x] `mgmt.vw_SwTestMap` - View for listing all tests with IsActive flag (LastRun within 3 months)
- [x] `mgmt.SwTestMap_GetById` - Get single test by ID
- [x] `mgmt.SwTestMap_Insert` - Insert new test, return identity via OUTPUT
- [x] `mgmt.SwTestMap_Update` - Update existing test

### floor.CellBySwTest Table Operations (`05_cellbyswtest_operations.sql`)
- [x] `mgmt.IntList` - Table-valued parameter type for passing lists of CellIds
- [x] `mgmt.CellBySwTest_GetBySwTestMapId` - Get cell mappings for a test
- [x] `mgmt.CellBySwTest_SetMappings` - Replace all cell mappings for a test (atomic)

### product.TLA Table Operations (`06_tla_operations.sql`)
- [x] `mgmt.vw_TLA` - View with IsUsed indicator (checks activity.ProductionTest)
- [x] `mgmt.TLA_GetByPartNo` - Get single TLA by PartNo with IsUsed
- [x] `mgmt.TLA_Insert` - Insert new TLA
- [x] `mgmt.TLA_Update` - Update existing TLA
- [x] `mgmt.TLA_Delete` - Hard delete unused TLA (validates not in use)

### floor.CellByPartNo Table Operations (`07_cellbypartno_operations.sql`)
- [x] `mgmt.CellByPartNo_GetByPartNo` - Get cell mappings for a part number
- [x] `mgmt.CellByPartNo_SetMappings` - Replace all cell mappings for a part number (atomic)

### Supporting Documentation
- [x] `ERROR_CODES.md` - Error code reference with user-friendly message mappings
- [x] `99_deploy_all.sql` - Master deployment script (reference)

### Phase 1 Review
- [x] All SQL scripts created in `docs/sql/`
- [ ] Scripts reviewed and approved by user

---

## Phase 2: Data API Layer

### Project Setup
- [ ] Create `src/core/PEMetrics.DataApi` class library project
- [ ] Add to solution
- [ ] Configure ADO.NET dependencies (System.Data.SqlClient)
- [ ] Create connection string management

### Interfaces (Ports)
- [ ] `ICellRepository` - Cell data operations
- [ ] `IPCStationRepository` - PCStation data operations
- [ ] `ICellByPCStationRepository` - PC to Cell mapping operations
- [ ] `ISwTestMapRepository` - Software test operations
- [ ] `ICellBySwTestRepository` - SwTest to Cell mapping operations
- [ ] `ITLARepository` - Part number operations
- [ ] `ICellByPartNoRepository` - PartNo to Cell mapping operations

### Domain Models
- [ ] `Cell` record/class
- [ ] `PCStation` record/class
- [ ] `CellByPCStation` record/class
- [ ] `SwTestMap` record/class
- [ ] `CellBySwTest` record/class
- [ ] `TLA` record/class
- [ ] `CellByPartNo` record/class

### Repository Implementations
- [ ] `CellRepository` implementation
- [ ] `PCStationRepository` implementation
- [ ] `CellByPCStationRepository` implementation
- [ ] `SwTestMapRepository` implementation
- [ ] `CellBySwTestRepository` implementation
- [ ] `TLARepository` implementation
- [ ] `CellByPartNoRepository` implementation

### Error Handling
- [ ] Custom exception types for business rule violations
- [ ] SQL error code translation

### Phase 2 Review
- [ ] All repository classes implemented
- [ ] Reviewed and approved by user

---

## Phase 3: Caching Layer

### Project Setup
- [ ] Create `src/core/PEMetrics.Cache` class library project
- [ ] Add to solution
- [ ] Add DuckDB.NET.Data.Full NuGet package
- [ ] Add reference to PEMetrics.DataApi project

### Cache Infrastructure
- [ ] `CacheConfiguration` - Path management, initialization
- [ ] `DuckDbConnectionFactory` - Connection management
- [ ] Create DuckDB tables mirroring dimension tables

### Cache Services
- [ ] `ICacheService` interface
- [ ] `CellCacheService` - Cell cache operations
- [ ] `PCStationCacheService` - PCStation cache operations
- [ ] `CellByPCStationCacheService` - Mapping cache operations
- [ ] `SwTestMapCacheService` - SwTest cache operations
- [ ] `CellBySwTestCacheService` - SwTest mapping cache
- [ ] `TLACacheService` - TLA cache operations
- [ ] `CellByPartNoCacheService` - PartNo mapping cache

### Cache Refresh
- [ ] Initial population from SQL Server
- [ ] Refresh methods for each table
- [ ] Cache invalidation strategies

### Phase 3 Review
- [ ] All cache services implemented
- [ ] Reviewed and approved by user

---

## Phase 4: Windows Forms UI Layer

### Project Setup
- [ ] Update existing `DimensionManagement` project
- [ ] Add references to DataApi and Cache projects
- [ ] Configure dependency injection
- [ ] Create main form with navigation

### Screen 1: Cell Management
- [ ] Cell list DataGridView with sorting
- [ ] Active Only filter
- [ ] Add/Edit dialog
- [ ] Integration with cache and data API
- [ ] Review and approval

### Screen 2: PC Station Management
- [ ] PC name input with autocomplete/suggestions
- [ ] Exists check on input
- [ ] Add confirmation dialog
- [ ] Review and approval

### Screen 3: PC to Cell Mapping Management
- [ ] Mapping list DataGridView with Cell names
- [ ] Active Only filter
- [ ] Add/Edit dialog with Cell picker
- [ ] Review and approval

### Screen 4: Software Test Management
- [ ] SwTestMap list DataGridView
- [ ] Active filter (LastRun within 3 months)
- [ ] Add/Edit dialog with Cell checkbox list
- [ ] Review and approval

### Screen 5: Part Number (TLA) Management
- [ ] TLA list DataGridView with IsUsed column
- [ ] Text search filter (Family, Subfamily, Description)
- [ ] IsUsed filter
- [ ] Add/Edit dialog with Cell checkbox list
- [ ] Delete button (disabled when IsUsed)
- [ ] Review and approval

---

## Notes

### Decisions Made

**Phase 1:**
- Used views (not TVFs) for read-only grid sources - simpler and sufficient for this use case
- Views include computed `IsActive` flags to support filtering in the UI
- Used Table-Valued Parameters (`mgmt.IntList`) for bulk cell mapping operations
- SetMappings procedures use atomic delete-then-insert pattern within transactions
- Error codes organized by entity ranges (50001-50009 for Cell, etc.)
- All RAISERROR calls use severity 16 for business rule violations

### Issues/Blockers
- (None at this time)

### Testing Notes
- Scripts can be deployed to PE_Metrics database using SSMS
- Run `00_schema_setup.sql` first, then remaining scripts in numeric order
- Or concatenate all scripts and run as single batch
