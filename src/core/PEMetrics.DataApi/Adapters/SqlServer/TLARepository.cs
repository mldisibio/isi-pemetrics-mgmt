using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingPartNumbers.</summary>
public sealed class TLARepository : ForManagingPartNumbers
{
    readonly ForCreatingSqlServerConnections _connectionFactory;

    public TLARepository(ForCreatingSqlServerConnections connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IReadOnlyCollection<TLA> GetAll()
    {
        throw new NotImplementedException();
    }

    public TLA? GetByPartNo(string partNo)
    {
        throw new NotImplementedException();
    }

    public void Insert(TLA tla)
    {
        throw new NotImplementedException();
    }

    public void Update(TLA tla)
    {
        throw new NotImplementedException();
    }

    public void Delete(string partNo)
    {
        throw new NotImplementedException();
    }
}
