using Microsoft.Data.SqlClient;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Converts ADO.NET SQL Server connection strings to ODBC format for nanodbc.</summary>
public static class OdbcConnectionStringBuilder
{
    /// <summary>Converts an ADO.NET connection string to ODBC format.</summary>
    public static string ToOdbcConnectionString(string adoNetConnectionString)
    {
        var builder = new SqlConnectionStringBuilder(adoNetConnectionString);

        var parts = new List<string>
        {
            "Driver={SQL Server}"
        };

        if (!string.IsNullOrEmpty(builder.DataSource))
            parts.Add($"Server={builder.DataSource}");

        if (!string.IsNullOrEmpty(builder.InitialCatalog))
            parts.Add($"Database={builder.InitialCatalog}");

        if (builder.IntegratedSecurity)
        {
            parts.Add("Trusted_Connection=yes");
        }
        else
        {
            if (!string.IsNullOrEmpty(builder.UserID))
                parts.Add($"Uid={builder.UserID}");

            if (!string.IsNullOrEmpty(builder.Password))
                parts.Add($"Pwd={builder.Password}");
        }

        if (builder.TrustServerCertificate)
            parts.Add("TrustServerCertificate=yes");

        return string.Join(";", parts) + ";";
    }
}
