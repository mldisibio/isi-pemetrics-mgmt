using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>
/// SQL Server implementation of ITLARepository.
/// </summary>
public sealed class TLARepository : ITLARepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public TLARepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IEnumerable<TLA> GetAll()
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
