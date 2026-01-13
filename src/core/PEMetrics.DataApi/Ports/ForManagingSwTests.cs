using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Ports;

/// <summary>Port for managing software test mappings.</summary>
public interface ForManagingSwTests
{
    /// <summary>Inserts a new software test.</summary>
    /// <returns>The new SwTestMapId.</returns>
    int Insert(SwTestMap test);

    /// <summary>Updates an existing software test.</summary>
    void Update(SwTestMap test);
}
