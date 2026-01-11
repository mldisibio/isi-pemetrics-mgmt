using System.Data.Common;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Infrastructure.Mapping;

/// <summary>Mapping functions for Cell models.</summary>
public interface ForMappingCellModels
{
    /// <summary>Maps a DbDataReader row to a Cell.</summary>
    Cell MapCell(DbDataReader reader);
}
