using System.Data.Common;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Infrastructure.Mapping;

public interface ForMappingDataModels
{
    /// <summary>Maps a DbDataReader row to a Cell.</summary>
    Cell MapCell(DbDataReader reader);
    /// <summary>Maps a DbDataReader row to a PCStation.</summary>
    PCStation MapPCStation(DbDataReader reader);
    /// <summary>Maps a DbDataReader row to a CellByPCStation.</summary>
    CellByPCStation MapCellByPCStation(DbDataReader reader);
    /// <summary>Maps a DbDataReader row to a SwTestMap.</summary>
    SwTestMap MapSwTestMap(DbDataReader reader);
    /// <summary>Maps a DbDataReader row to a CellBySwTest (simple mapping record).</summary>
    CellBySwTest MapCellBySwTest(DbDataReader reader);
    /// <summary>Maps a DbDataReader row to a CellBySwTestView (expanded view data).</summary>
    CellBySwTestView MapCellBySwTestView(DbDataReader reader);
    /// <summary>Maps a DbDataReader row to a TLA.</summary>
    TLA MapTLA(DbDataReader reader);
    /// <summary>Maps a DbDataReader row to a CellByPartNo (simple mapping record).</summary>
    CellByPartNo MapCellByPartNo(DbDataReader reader);
    /// <summary>Maps a DbDataReader row to a CellByPartNoView (expanded view data).</summary>
    CellByPartNoView MapCellByPartNoView(DbDataReader reader);
}