using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Adapters.SqlServer;

/// <summary>SQL Server implementation of ForMappingPCStationToCell.</summary>
public sealed class CellByPCStationRepository : ForMappingPCStationToCell
{
    readonly ForCreatingSqlServerConnections _connectionFactory;

    public CellByPCStationRepository(ForCreatingSqlServerConnections connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IReadOnlyCollection<CellByPCStation> GetAll()
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
