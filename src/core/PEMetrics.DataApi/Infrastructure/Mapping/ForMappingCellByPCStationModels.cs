using System.Data.Common;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Infrastructure.Mapping;

/// <summary>Mapping functions for CellByPCStation models.</summary>
public interface ForMappingCellByPCStationModels
{
    /// <summary>Maps a DbDataReader row to a CellByPCStation.</summary>
    CellByPCStation MapCellByPCStation(DbDataReader reader);
}
