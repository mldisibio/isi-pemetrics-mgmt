using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataCache.Services;

/// <summary>Checks SQL Server connectivity at startup and notifies on failure.</summary>
public sealed class ProductionStoreHealthCheck
{
    readonly string _connectionString;
    readonly ForNotifyingDataCommunicationErrors _errorNotifier;

    /// <summary>SQL error numbers that indicate network/timeout issues.</summary>
    static readonly int[] NetworkErrorNumbers = [-1, -2, 2, 53];

    public ProductionStoreHealthCheck(
        IConfiguration configuration,
        ForNotifyingDataCommunicationErrors errorNotifier)
    {
        _connectionString = configuration.GetConnectionString("PEMetricsConnection")
            ?? throw new InvalidOperationException("PEMetricsConnection connection string not found.");
        _errorNotifier = errorNotifier ?? throw new ArgumentNullException(nameof(errorNotifier));
    }

    /// <summary>Tests SQL Server connectivity. Returns true if reachable, false if not.</summary>
    /// <remarks>On failure, calls ProductionStoreNotReachable notification. Does not throw.</remarks>
    public bool TestConnectivity()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 10;
            command.ExecuteScalar();

            return true;
        }
        catch (SqlException ex) when (IsNetworkError(ex))
        {
            _errorNotifier.ProductionStoreNotReachable(ex);
            return false;
        }
        catch (Exception ex)
        {
            _errorNotifier.ProductionStoreNotReachable(ex);
            return false;
        }
    }

    /// <summary>Tests SQL Server connectivity asynchronously. Returns true if reachable, false if not.</summary>
    public async Task<bool> TestConnectivityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 10;
            await command.ExecuteScalarAsync(cancellationToken);

            return true;
        }
        catch (SqlException ex) when (IsNetworkError(ex))
        {
            _errorNotifier.ProductionStoreNotReachable(ex);
            return false;
        }
        catch (Exception ex)
        {
            _errorNotifier.ProductionStoreNotReachable(ex);
            return false;
        }
    }

    static bool IsNetworkError(SqlException ex)
    {
        return NetworkErrorNumbers.Contains(ex.Number);
    }
}
