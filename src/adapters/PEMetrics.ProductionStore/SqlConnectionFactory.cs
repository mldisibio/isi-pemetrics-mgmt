using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PEMetrics.DataApi.Infrastructure;

namespace PEMetrics.ProductionStore;

/// <summary>SQL Server connection factory implementation.</summary>
public sealed class SqlConnectionFactory : ForCreatingSqlServerConnections
{
    readonly IConfiguration _configuration;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public DbConnection OpenConnectionToPEMetrics()
    {
        var connectionString = _configuration.GetConnectionString("PEMetricsConnection")
            ?? throw new InvalidOperationException("Connection string 'PEMetricsConnection' not found in configuration.");

        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}
