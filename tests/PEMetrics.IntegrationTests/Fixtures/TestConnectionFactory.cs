using System.Data.Common;
using Microsoft.Data.SqlClient;
using PEMetrics.DataApi.Infrastructure;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Test implementation of ForCreatingSqlServerConnections using the container connection string.</summary>
public sealed class TestConnectionFactory : ForCreatingSqlServerConnections
{
    readonly string _connectionString;

    public TestConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbConnection OpenConnectionToPEMetrics()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
