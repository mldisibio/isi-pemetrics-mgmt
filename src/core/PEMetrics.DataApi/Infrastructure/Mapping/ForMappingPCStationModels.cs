using System.Data.Common;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Infrastructure.Mapping;

/// <summary>Mapping functions for PCStation models.</summary>
public interface ForMappingPCStationModels
{
    /// <summary>Maps a DbDataReader row to a PCStation.</summary>
    PCStation MapPCStation(DbDataReader reader);
}
