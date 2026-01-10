using System.Collections.Immutable;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing PC-to-Cell mappings.</summary>
public interface ForMappingPCStationToCell
{
    /// <summary>Retrieves all PC-to-Cell mappings from mgmt.vw_CellByPCStation.</summary>
    ImmutableList<CellByPCStation> GetAll();

    /// <summary>Retrieves a single mapping by ID.</summary>
    /// <returns>The mapping, or null if not found.</returns>
    CellByPCStation? GetById(int stationMapId);

    /// <summary>Inserts a new PC-to-Cell mapping.</summary>
    /// <returns>The new StationMapId.</returns>
    int Insert(CellByPCStation mapping);

    /// <summary>Updates an existing PC-to-Cell mapping.</summary>
    void Update(CellByPCStation mapping);
}
