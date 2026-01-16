using System.Collections.Immutable;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for reading all PE Metrics dimensions.</summary>
public interface ForReadingPEMetricsDimensions
{
    /// <summary>Retrieves all cells from mgmt.vw_Cell.</summary>
    Task<ImmutableList<Cell>> GetCellsAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single cell by ID.</summary>
    /// <returns>The cell, or null if not found.</returns>
    Task<Cell?> GetCellByIdAsync(int cellId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all PC stations from mgmt.vw_PCStation.</summary>
    Task<ImmutableList<PCStation>> GetPCStationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Searches PC stations by name prefix (for autocomplete).</summary>
    /// <returns>Matching PC stations sorted alphabetically.</returns>
    Task<ImmutableList<PCStation>> SearchPCStationsAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all PC-to-Cell mappings from mgmt.vw_CellByPCStation.</summary>
    Task<ImmutableList<CellByPCStation>> GetPcToCellMappingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single mapping by ID.</summary>
    /// <returns>The mapping, or null if not found.</returns>
    Task<CellByPCStation?> GetPcToCellByMapIdAsync(int stationMapId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all software tests from mgmt.vw_SwTestMap.</summary>
    Task<ImmutableList<SwTestMap>> GetSwTestsAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single software test by ID.</summary>
    /// <returns>The test, or null if not found.</returns>
    Task<SwTestMap?> GetSwTestByIdAsync(int swTestMapId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all software test to cell mappings from mgmt.vw_CellBySwTest.</summary>
    Task<ImmutableList<CellBySwTestView>> GetSwTestToCellMappingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves all cell mappings for a specific software test.</summary>
    Task<ImmutableList<CellBySwTest>> GetSwTestToCellByMapIdAsync(int swTestMapId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all TLAs from mgmt.vw_TLA.</summary>
    Task<ImmutableList<TLA>> GetTLACatalogAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single TLA by part number.</summary>
    /// <returns>The TLA, or null if not found.</returns>
    Task<TLA?> GetTLAByPartNoAsync(string partNo, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all part number to cell mappings from mgmt.vw_CellByPartNo.</summary>
    Task<ImmutableList<CellByPartNoView>> GetTLAToCellMappingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves all cell mappings for a specific part number.</summary>
    Task<ImmutableList<CellByPartNo>> GetTLAToCellByPartNoAsync(string partNo, CancellationToken cancellationToken = default);
}
