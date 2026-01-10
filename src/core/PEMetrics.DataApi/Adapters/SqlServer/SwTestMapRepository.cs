using System.Collections.Immutable;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingSwTests.</summary>
public sealed class SwTestMapRepository : ForManagingSwTests
{
    readonly ForCreatingSqlServerConnections _connectionFactory;

    public SwTestMapRepository(ForCreatingSqlServerConnections connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public ImmutableList<SwTestMap> GetAll()
    {
        throw new NotImplementedException();
    }

    public SwTestMap? GetById(int swTestMapId)
    {
        throw new NotImplementedException();
    }

    public int Insert(SwTestMap test)
    {
        throw new NotImplementedException();
    }

    public void Update(SwTestMap test)
    {
        throw new NotImplementedException();
    }
}
