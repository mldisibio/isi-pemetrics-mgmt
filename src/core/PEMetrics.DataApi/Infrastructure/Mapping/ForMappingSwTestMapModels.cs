using System.Data.Common;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Infrastructure.Mapping;

/// <summary>Mapping functions for SwTestMap models.</summary>
public interface ForMappingSwTestMapModels
{
    /// <summary>Maps a DbDataReader row to a SwTestMap.</summary>
    SwTestMap MapSwTestMap(DbDataReader reader);
}
