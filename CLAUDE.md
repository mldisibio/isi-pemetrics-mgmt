# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Global Context**: See `~/source/mldisibio/CLAUDE.md` for organizational standards and preferences that apply to all projects.

## Project Overview

- This is a .NET 9.0 Windows Forms application for CRUD operations on PE_Metrics dimensions. The starting bare bones solution contains a GUI application for dimension management.
- "PE" refers to "Production Engineering" and reflects the context of software that tests water quality devices with sensors in a manufacturing environment.
- PE_Metrics is a star schema stored in MS Sql Server and consumed by Power BI. The dimensions ('Cell', 'PCStation', 'SoftwareTest', 'Product Number') each require manual maintenance which user currently does using raw sql, but it is tedious due to intentional foreign key constraints and the need to often check what already exists and then write cumbersome sql to insert new data or update existing rows.
- This project is concerned only with the dimension table maintainence. The larger context is that fact table tracks the "Pass/Fail" outcome of each test run and this is used for manufacturing metrics such as "First Pass Yield" as broken down by each dimension.
- All data modification operations are executed via MS Sql Server stored procedures.
- Stored procedures and views are part of the project deliverables and are versioned in the repository.
- The MS Sql Server `PE_Metrics` database is the source of truth for validation and enforcement of business rules.

## Technology Stack

- **Framework**: .NET 9.0 (net9.0-windows)
- **UI**: Windows Forms
- **Language Features**: C# with nullable reference types enabled, implicit usings enabled

## Project Structure

- `PE_Metrics_DataMgmt.sln` - Main Visual Studio solution file
- `src/gui/DimensionManagement/` - Windows Forms GUI application for dimension management

As we iterate the project, business logic will be separate from the UI, so classes should be stored under `src/core` and possibly incorporate "Ports and Adapters" folder structures if not too clumsy.

Also consider that a possible future iteration would use the same business logic but the ui will be terminal based ("tui") as a learning experiment, stored under `src/tui/DimensionMangement`.

If XUnit tests are added (not a strict requirement) they would be stored in a `tests` directory parallel to `src`

## Database Interaction Model

- All create, update, and delete operations must be executed via MS Sql Server stored procedures.
- Stored procedures are responsible for:
  - validating business rules
  - enforcing invariants
  - performing atomic writes
- The application layer must not issue ad-hoc INSERT, UPDATE, or DELETE statements.

- Read-only data for UI grids must be sourced from database views or table-valued functions designed for display purposes.
- The inline database (DuckDB) is a read-only cache and must not be used as a write target.
- All write operations are executed by the application layer directly against SQL Server via stored procedures.
- Cache refresh logic is coordinated by the application layer after successful command execution.


## Common Commands

### Build
```bash
dotnet build PE_Metrics_DataMgmt.sln
```

Build specific configuration:
```bash
dotnet build PE_Metrics_DataMgmt.sln -c Release
dotnet build PE_Metrics_DataMgmt.sln -c Debug
```

### Run
```bash
dotnet run --project src/gui/DimensionManagement/DimensionManagement.csproj
```

### Clean
```bash
dotnet clean PE_Metrics_DataMgmt.sln
```
### Nuget Packages
```bash
dotnet add package DuckDB.NET.Data.Full
```
### Restore Dependencies
```bash
dotnet restore PE_Metrics_DataMgmt.sln
```

## Documentation

- `REQUIREMENTS.md` - Functional requirements and specifications for the application
- `DATABASE.md` - Database schema and table structures for PE_Metrics dimensions

## Async Programming

This project follows async patterns throughout the data layer. When implementing new features:

- **Prefer async over sync**: Use async methods even if not explicitly stated in requirements
- **Use `.ConfigureAwait(false)`**: All `await` calls in library code should use `.ConfigureAwait(false)` per Microsoft best practices
- **Propagate CancellationToken**: All async methods should accept `CancellationToken cancellationToken = default`
- **Use async disposal**: Prefer `await using` over `using` for disposable async resources
- **Async ADO.NET methods**: Use `OpenAsync`, `ExecuteReaderAsync`, `ExecuteNonQueryAsync`, `ExecuteScalarAsync`
- **Naming convention**: Async methods should have the `Async` suffix

Example pattern:
```csharp
public async Task<ImmutableList<T>> GetItemsAsync(CancellationToken cancellationToken = default)
{
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM Items";
    await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
    return await reader.MapAllAsync(mapper, cancellationToken).ConfigureAwait(false);
}
```

## Development Notes

- The solution uses Visual Studio 2017 format (Version 17.0+) but the IDE is Visual Studio 2022
- Build artifacts are placed in `bin/` and `obj/` directories (gitignored)
- The project targets Windows-specific APIs and cannot run on non-Windows platforms
