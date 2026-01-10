using Microsoft.Data.SqlClient;

namespace PEMetrics.DataApi.Infrastructure;

/// <summary>Factory for creating SQL Server connections.</summary>
public interface ForCreatingSqlServerConnections
{
    /// <summary>Creates and opens a connection to the PE_Metrics database.</summary>
    SqlConnection OpenConnectionToPEMetrics();
}
