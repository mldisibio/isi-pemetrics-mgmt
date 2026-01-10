using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>
/// Port for managing software test mappings.
/// </summary>
public interface ISwTestMapRepository
{
    /// <summary>
    /// Retrieves all software tests from mgmt.vw_SwTestMap.
    /// </summary>
    IEnumerable<SwTestMap> GetAll();

    /// <summary>
    /// Retrieves a single software test by ID.
    /// </summary>
    /// <returns>The test, or null if not found.</returns>
    SwTestMap? GetById(int swTestMapId);

    /// <summary>
    /// Inserts a new software test.
    /// </summary>
    /// <returns>The new SwTestMapId.</returns>
    int Insert(SwTestMap test);

    /// <summary>
    /// Updates an existing software test.
    /// </summary>
    void Update(SwTestMap test);
}
