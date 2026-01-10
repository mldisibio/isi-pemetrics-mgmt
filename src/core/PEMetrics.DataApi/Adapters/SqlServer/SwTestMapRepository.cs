using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>
/// SQL Server implementation of ISwTestMapRepository.
/// </summary>
public sealed class SwTestMapRepository : ISwTestMapRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SwTestMapRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IEnumerable<SwTestMap> GetAll()
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
