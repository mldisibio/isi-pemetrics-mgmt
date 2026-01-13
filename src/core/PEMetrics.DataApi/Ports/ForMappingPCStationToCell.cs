using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing PC-to-Cell mappings.</summary>
public interface ForMappingPCStationToCell
{
    /// <summary>Inserts a new PC-to-Cell mapping.</summary>
    /// <returns>The new StationMapId.</returns>
    int Insert(CellByPCStation mapping);

    /// <summary>Updates an existing PC-to-Cell mapping.</summary>
    void Update(CellByPCStation mapping);
}
