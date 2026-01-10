using Microsoft.Data.SqlClient;

namespace PEMetrics.DataApi.Infrastructure;

/// <summary>
/// Translates SQL Server error codes to user-friendly RepositoryExceptions.
/// </summary>
public static class SqlErrorTranslator
{
    /// <summary>
    /// Translates a SqlException to a RepositoryException if it contains a known error code.
    /// </summary>
    /// <param name="ex">The SqlException to translate.</param>
    /// <returns>A RepositoryException with a user-friendly message, or rethrows if unknown.</returns>
    public static RepositoryException Translate(SqlException ex)
    {
        var errorCode = ex.Number;
        var message = GetUserMessage(errorCode);
        return new RepositoryException(errorCode, message, ex);
    }

    /// <summary>
    /// Gets a user-friendly message for a SQL error code.
    /// </summary>
    public static string GetUserMessage(int errorCode) => errorCode switch
    {
        // Cell
        50001 => "A cell with this name already exists. Please choose a different name.",
        50002 => "A cell with this display name already exists. Please choose a different display name.",
        50003 => "The cell you are trying to update no longer exists.",

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

        _ => $"A database error occurred (code {errorCode}). Please try again or contact support."
    };
}
