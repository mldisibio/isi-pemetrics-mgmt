using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>
/// SQL Server implementation of ICellRepository.
/// </summary>
public sealed class CellRepository : ICellRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public CellRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IEnumerable<Cell> GetAll()
    {
        throw new NotImplementedException();
    }

    public Cell? GetById(int cellId)
    {
        throw new NotImplementedException();
    }

    public int Insert(Cell cell)
    {
        throw new NotImplementedException();
    }

    public void Update(Cell cell)
    {
        throw new NotImplementedException();
    }
}
