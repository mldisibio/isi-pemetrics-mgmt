using System.Data.Common;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Infrastructure.Mapping;

/// <summary>Mapping functions for CellByPartNo models.</summary>
public interface ForMappingCellByPartNoModels
{
    /// <summary>Maps a DbDataReader row to a CellByPartNo (simple mapping record).</summary>
    CellByPartNo MapCellByPartNo(DbDataReader reader);

    /// <summary>Maps a DbDataReader row to a CellByPartNoView (expanded view data).</summary>
    CellByPartNoView MapCellByPartNoView(DbDataReader reader);
}
