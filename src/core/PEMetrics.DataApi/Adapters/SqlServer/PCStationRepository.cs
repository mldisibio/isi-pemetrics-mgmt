using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>
/// SQL Server implementation of IPCStationRepository.
/// </summary>
public sealed class PCStationRepository : IPCStationRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public PCStationRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IEnumerable<PCStation> GetAll()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<PCStation> Search(string prefix)
    {
        throw new NotImplementedException();
    }

    public void Insert(string pcName)
    {
        throw new NotImplementedException();
    }
}
