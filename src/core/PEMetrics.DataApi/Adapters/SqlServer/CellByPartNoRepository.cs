using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>
/// SQL Server implementation of ICellByPartNoRepository.
/// </summary>
public sealed class CellByPartNoRepository : ICellByPartNoRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public CellByPartNoRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IEnumerable<CellByPartNo> GetByPartNo(string partNo)
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
