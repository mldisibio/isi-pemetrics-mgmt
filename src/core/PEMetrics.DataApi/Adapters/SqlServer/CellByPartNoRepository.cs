using System.Collections.Immutable;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForMappingPartNumberToCells.</summary>
public sealed class CellByPartNoRepository : ForMappingPartNumberToCells
{
    readonly ForCreatingSqlServerConnections _connectionFactory;

    public CellByPartNoRepository(ForCreatingSqlServerConnections connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public ImmutableList<CellByPartNo> GetByPartNo(string partNo)
    {
        throw new NotImplementedException();
    }

    public void SetMappings(string partNo, IEnumerable<int> cellIds)
    {
        throw new NotImplementedException();
    }

    public void AddMapping(string partNo, int cellId)
    {
        throw new NotImplementedException();
    }

    public void DeleteMapping(string partNo, int cellId)
    {
        throw new NotImplementedException();
    }
}
