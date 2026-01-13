using System.Collections.Immutable;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for reading all PE Metrics dimensions.</summary>
public interface ForReadingPEMetricsDimensions
{
    /// <summary>Retrieves all cells from mgmt.vw_Cell.</summary>
    ImmutableList<Cell> GetCells();

    /// <summary>Retrieves a single cell by ID.</summary>
    /// <returns>The cell, or null if not found.</returns>
    Cell? GetCellById(int cellId);

    /// <summary>Retrieves all PC stations from mgmt.vw_PCStation.</summary>
    ImmutableList<PCStation> GetPCStations();

    /// <summary>Searches PC stations by name prefix (for autocomplete).</summary>
    /// <returns>Matching PC stations sorted alphabetically.</returns>
    ImmutableList<PCStation> SearchPCStations(string prefix);

    /// <summary>Retrieves all PC-to-Cell mappings from mgmt.vw_CellByPCStation.</summary>
    ImmutableList<CellByPCStation> GetPcToCellMappings();

    /// <summary>Retrieves a single mapping by ID.</summary>
    /// <returns>The mapping, or null if not found.</returns>
    CellByPCStation? GetPcToCellByMapId(int stationMapId);

    /// <summary>Retrieves all software tests from mgmt.vw_SwTestMap.</summary>
    ImmutableList<SwTestMap> GetSwTests();

    /// <summary>Retrieves a single software test by ID.</summary>
    /// <returns>The test, or null if not found.</returns>
    SwTestMap? GetSwTestById(int swTestMapId);

    /// <summary>Retrieves all software test to cell mappings from mgmt.vw_CellBySwTest.</summary>
    ImmutableList<CellBySwTestView> GetSwTestToCellMappings();

    /// <summary>Retrieves all cell mappings for a specific software test.</summary>
    ImmutableList<CellBySwTest> GetSwTestToCellByMapId(int swTestMapId);

    /// <summary>Retrieves all TLAs from mgmt.vw_TLA.</summary>
    ImmutableList<TLA> GetTLACatalog();

    /// <summary>Retrieves a single TLA by part number.</summary>
    /// <returns>The TLA, or null if not found.</returns>
    TLA? GetTLAByPartNo(string partNo);

    /// <summary>Retrieves all part number to cell mappings from mgmt.vw_CellByPartNo.</summary>
    ImmutableList<CellByPartNoView> GetTLAToCellMappings();

    /// <summary>Retrieves all cell mappings for a specific part number.</summary>
    ImmutableList<CellByPartNo> GetTLAToCellByPartNo(string partNo);
}
