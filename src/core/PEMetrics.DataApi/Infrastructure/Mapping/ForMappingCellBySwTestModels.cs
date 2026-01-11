using System.Data.Common;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Infrastructure.Mapping;

/// <summary>Mapping functions for CellBySwTest models.</summary>
public interface ForMappingCellBySwTestModels
{
    /// <summary>Maps a DbDataReader row to a CellBySwTest (simple mapping record).</summary>
    CellBySwTest MapCellBySwTest(DbDataReader reader);

    /// <summary>Maps a DbDataReader row to a CellBySwTestView (expanded view data).</summary>
    CellBySwTestView MapCellBySwTestView(DbDataReader reader);
}
