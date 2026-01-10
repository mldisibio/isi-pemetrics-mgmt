# SQL Server Error Codes Reference

This document lists all custom error codes used by the PE_Metrics Dimension Management stored procedures.

## Error Code Ranges

| Range | Entity |
|-------|--------|
| 50001-50009 | Cell |
| 50010-50019 | PCStation |
| 50020-50029 | CellByPCStation |
| 50030-50039 | SwTestMap / CellBySwTest |
| 50040-50049 | TLA / CellByPartNo |

---

## Cell Errors (50001-50009)

| Code | Message | Description |
|------|---------|-------------|
| 50001 | CellName already exists | Attempted to insert/update with a CellName that is already used by another cell |
| 50002 | DisplayName already exists | Attempted to insert/update with a DisplayName that is already used by another cell |
| 50003 | Cell not found | Attempted to update a cell that does not exist |

---

## PCStation Errors (50010-50019)

No errors are raised for PCStation operations. The insert procedure is idempotent
(silently succeeds if the name already exists).

---

## CellByPCStation Errors (50020-50029)

| Code | Message | Description |
|------|---------|-------------|
| 50020 | Cell not found | Referenced CellId does not exist in floor.Cell |
| 50021 | PC station not found | Referenced PcName does not exist in floor.PCStation |
| 50022 | Duplicate mapping | A mapping with the same CellId, PcName, and ActiveFrom already exists |
| 50023 | Mapping not found | Attempted to update a StationMapId that does not exist |

---

## SwTestMap / CellBySwTest Errors (50030-50039)

| Code | Message | Description |
|------|---------|-------------|
| 50030 | Software test not found | Referenced SwTestMapId does not exist in sw.SwTestMap |
| 50031 | One or more cells not found | One or more CellIds in the mapping list do not exist |
| 50032 | Duplicate ConfiguredTestId and TestName | A software test with this combination already exists |

---

## TLA / CellByPartNo Errors (50040-50049)

| Code | Message | Description |
|------|---------|-------------|
| 50040 | Part number already exists | Attempted to insert a PartNo that already exists |
| 50041 | Part number not found | Attempted to update a PartNo that does not exist (Delete is idempotent) |
| 50042 | Cannot delete: referenced in production tests | Attempted to delete a PartNo that has records in activity.ProductionTest |
| 50043 | Cannot delete: referenced in cell mappings | Attempted to delete a PartNo that still has cell mappings |

---

## User-Friendly Message Mapping

The application layer should translate these error codes to user-friendly messages:

```csharp
public static class SqlErrorMessages
{
    public static string GetUserMessage(int errorCode) => errorCode switch
    {
        // Cell
        50001 => "A cell with this name already exists. Please choose a different name.",
        50002 => "A cell with this display name already exists. Please choose a different display name.",
        50003 => "The cell you are trying to update no longer exists.",

        // PCStation - No errors (insert is idempotent)

        // CellByPCStation
        50020 => "The selected cell does not exist.",
        50021 => "The selected PC station does not exist.",
        50022 => "A mapping for this PC station, cell, and start date already exists.",
        50023 => "The mapping you are trying to update no longer exists.",

        // SwTestMap / CellBySwTest
        50030 => "The software test you are trying to update no longer exists.",
        50031 => "One or more selected cells do not exist.",
        50032 => "A software test with this Configured Test ID and Test Name already exists.",

        // TLA / CellByPartNo
        50040 => "This part number already exists in the system.",
        50041 => "The part number you are trying to update no longer exists.",
        50042 => "This part number cannot be deleted because it has production test records.",
        50043 => "This part number cannot be deleted while it has cell assignments. Remove the cell assignments first.",

        _ => "An unexpected database error occurred. Please try again or contact support."
    };
}
```
