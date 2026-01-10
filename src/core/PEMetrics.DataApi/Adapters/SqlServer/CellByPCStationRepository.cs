using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>
/// SQL Server implementation of ICellByPCStationRepository.
/// </summary>
public sealed class CellByPCStationRepository : ICellByPCStationRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public CellByPCStationRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IEnumerable<CellByPCStation> GetAll()
    {
        throw new NotImplementedException();
    }

    public CellByPCStation? GetById(int stationMapId)
    {
        throw new NotImplementedException();
    }

    public int Insert(CellByPCStation mapping)
    {
        throw new NotImplementedException();
    }

    public void Update(CellByPCStation mapping)
    {
        throw new NotImplementedException();
    }
}
