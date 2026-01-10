using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForManagingCells.</summary>
public sealed class CellRepository : ForManagingCells
{
    readonly ForCreatingSqlServerConnections _connectionFactory;

    public CellRepository(ForCreatingSqlServerConnections connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IReadOnlyCollection<Cell> GetAll()
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
