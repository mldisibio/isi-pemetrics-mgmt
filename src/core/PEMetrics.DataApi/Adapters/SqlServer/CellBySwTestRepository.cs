using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>
/// SQL Server implementation of ICellBySwTestRepository.
/// </summary>
public sealed class CellBySwTestRepository : ICellBySwTestRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public CellBySwTestRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IEnumerable<CellBySwTest> GetBySwTestMapId(int swTestMapId)
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
