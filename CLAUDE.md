# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Global Context**: See `~/source/mldisibio/CLAUDE.md` for organizational standards and preferences that apply to all projects.

## Project Overview

- This is a .NET 9.0 Windows Forms application for CRUD operations on PE_Metrics dimensions. The starting bare bones solution contains a GUI application for dimension management.
- "PE" refers to "Production Engineering" and reflects the context of software that tests water quality devices with sensors in a manufacturing environment.
- PE_Metrics is a star schema stored in MS Sql Server and consumed by Power BI. The dimensions ('Cell', 'PCStation', 'SoftwareTest', 'Product Number') each require manual maintenance which user currently does using raw sql, but it is tedious due to intentional foreign key constraints and the need to often check what already exists and then write cumbersome sql to insert new data or update existing rows.
- This project is concerned only with the dimension table maintainence. The larger context is that fact table tracks the "Pass/Fail" outcome of each test run and this is used for manufacturing metrics such as "First Pass Yield" as broken down by each dimension.

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

### Restore Dependencies
```bash
dotnet restore PE_Metrics_DataMgmt.sln
```

## Documentation

- `REQUIREMENTS.md` - Functional requirements and specifications for the application
- `DATABASE.md` - Database schema and table structures for PE_Metrics dimensions

## Development Notes

- The solution uses Visual Studio 2017 format (Version 17.0+) but the IDE is Visual Studio 2022
- Build artifacts are placed in `bin/` and `obj/` directories (gitignored)
- The project targets Windows-specific APIs and cannot run on non-Windows platforms
