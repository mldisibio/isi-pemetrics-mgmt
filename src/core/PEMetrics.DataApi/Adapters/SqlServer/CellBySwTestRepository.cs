using System.Collections.Immutable;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForMappingSwTestsToCells.</summary>
public sealed class CellBySwTestRepository : ForMappingSwTestsToCells
{
    readonly ForCreatingSqlServerConnections _connectionFactory;

    public CellBySwTestRepository(ForCreatingSqlServerConnections connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public ImmutableList<CellBySwTestView> GetAll()
    {
        throw new NotImplementedException();
    }

    public ImmutableList<CellBySwTest> GetBySwTestMapId(int swTestMapId)
    {
        throw new NotImplementedException();
    }

    public void SetMappings(int swTestMapId, IEnumerable<int> cellIds)
    {
        throw new NotImplementedException();
    }

    public void AddMapping(int swTestMapId, int cellId)
    {
        throw new NotImplementedException();
    }

    public void DeleteMapping(int swTestMapId, int cellId)
    {
        throw new NotImplementedException();
    }
}
