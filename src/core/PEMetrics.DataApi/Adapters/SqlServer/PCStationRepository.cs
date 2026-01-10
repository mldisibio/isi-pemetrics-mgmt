using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingPCStations.</summary>
public sealed class PCStationRepository : ForManagingPCStations
{
    readonly ForCreatingSqlServerConnections _connectionFactory;

    public PCStationRepository(ForCreatingSqlServerConnections connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IReadOnlyCollection<PCStation> GetAll()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<PCStation> Search(string prefix)
    {
        throw new NotImplementedException();
    }

    public void Insert(string pcName)
    {
        throw new NotImplementedException();
    }
}
