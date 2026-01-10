using Microsoft.Data.SqlClient;

namespace PEMetrics.DataApi.Infrastructure;

/// <summary>
/// Factory for creating SQL Server connections.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Creates and opens a new SQL connection.
    /// </summary>
    SqlConnection CreateConnection();
}
