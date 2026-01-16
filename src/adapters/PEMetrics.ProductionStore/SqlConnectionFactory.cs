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

    public async Task<DbConnection> OpenConnectionToPEMetricsAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("PEMetricsConnection")
            ?? throw new InvalidOperationException("Connection string 'PEMetricsConnection' not found in configuration.");

        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
