using System.Data.Common;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Infrastructure.Mapping;

/// <summary>Mapping functions for TLA models.</summary>
public interface ForMappingTLAModels
{
    /// <summary>Maps a DbDataReader row to a TLA.</summary>
    TLA MapTLA(DbDataReader reader);
}
