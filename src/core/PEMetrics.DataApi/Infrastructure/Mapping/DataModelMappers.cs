using System.Data.Common;
using PEMetrics.DataApi.Models;

namespace PEMetrics.DataApi.Infrastructure.Mapping;

/// <summary>Unified implementation of all data model mapping functions.</summary>
public sealed class DataModelMappers :
    ForMappingCellModels,
    ForMappingPCStationModels,
    ForMappingCellByPCStationModels,
    ForMappingSwTestMapModels,
    ForMappingCellBySwTestModels,
    ForMappingTLAModels,
    ForMappingCellByPartNoModels
{
    public Cell MapCell(DbDataReader reader) => new()
    {
        CellId = reader.GetInt32(reader.GetOrdinal("CellId")),
        CellName = reader.GetString(reader.GetOrdinal("CellName")),
        DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
        ActiveFrom = reader.GetDateOnly(reader.GetOrdinal("ActiveFrom")),
        ActiveTo = reader.GetNullableDateOnly(reader.GetOrdinal("ActiveTo")),
        Description = reader.GetNullableString(reader.GetOrdinal("Description")),
        AlternativeNames = reader.GetNullableString(reader.GetOrdinal("AlternativeNames")),
        IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1
    };

    public PCStation MapPCStation(DbDataReader reader) => new()
    {
        PcName = reader.GetString(reader.GetOrdinal("PcName"))
    };

    public CellByPCStation MapCellByPCStation(DbDataReader reader) => new()
    {
        StationMapId = reader.GetInt32(reader.GetOrdinal("StationMapId")),
        CellId = reader.GetInt32(reader.GetOrdinal("CellId")),
        CellName = reader.GetNullableString(reader.GetOrdinal("CellName")),
        PcName = reader.GetString(reader.GetOrdinal("PcName")),
        PcPurpose = reader.GetNullableString(reader.GetOrdinal("PcPurpose")),
        ActiveFrom = reader.GetDateOnly(reader.GetOrdinal("ActiveFrom")),
        ActiveTo = reader.GetNullableDateOnly(reader.GetOrdinal("ActiveTo")),
        ExtendedName = reader.GetNullableString(reader.GetOrdinal("ExtendedName")),
        IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1
    };

    public SwTestMap MapSwTestMap(DbDataReader reader) => new()
    {
        SwTestMapId = reader.GetInt32(reader.GetOrdinal("SwTestMapId")),
        ConfiguredTestId = reader.GetNullableString(reader.GetOrdinal("ConfiguredTestId")),
        TestApplication = reader.GetNullableString(reader.GetOrdinal("TestApplication")),
        TestName = reader.GetNullableString(reader.GetOrdinal("TestName")),
        ReportKey = reader.GetNullableString(reader.GetOrdinal("ReportKey")),
        TestDirectory = reader.GetNullableString(reader.GetOrdinal("TestDirectory")),
        RelativePath = reader.GetNullableString(reader.GetOrdinal("RelativePath")),
        LastRun = reader.GetNullableDateOnly(reader.GetOrdinal("LastRun")),
        Notes = reader.GetNullableString(reader.GetOrdinal("Notes")),
        IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1
    };

    public CellBySwTest MapCellBySwTest(DbDataReader reader) => new()
    {
        SwTestMapId = reader.GetInt32(reader.GetOrdinal("SwTestMapId")),
        CellId = reader.GetInt32(reader.GetOrdinal("CellId")),
        CellName = reader.GetNullableString(reader.GetOrdinal("CellName"))
    };

    public CellBySwTestView MapCellBySwTestView(DbDataReader reader) => new()
    {
        SwTestMapId = reader.GetInt32(reader.GetOrdinal("SwTestMapId")),
        ConfiguredTestId = reader.GetNullableString(reader.GetOrdinal("ConfiguredTestId")),
        TestApplication = reader.GetNullableString(reader.GetOrdinal("TestApplication")),
        ReportKey = reader.GetNullableString(reader.GetOrdinal("ReportKey")),
        LastRun = reader.GetNullableDateOnly(reader.GetOrdinal("LastRun")),
        IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
        CellId = reader.GetInt32(reader.GetOrdinal("CellId")),
        CellName = reader.GetNullableString(reader.GetOrdinal("CellName"))
    };

    public TLA MapTLA(DbDataReader reader) => new()
    {
        PartNo = reader.GetString(reader.GetOrdinal("PartNo")),
        Family = reader.GetNullableString(reader.GetOrdinal("Family")),
        Subfamily = reader.GetNullableString(reader.GetOrdinal("Subfamily")),
        ServiceGroup = reader.GetNullableString(reader.GetOrdinal("ServiceGroup")),
        FormalDescription = reader.GetNullableString(reader.GetOrdinal("FormalDescription")),
        Description = reader.GetNullableString(reader.GetOrdinal("Description")),
        IsUsed = reader.GetInt32(reader.GetOrdinal("IsUsed")) == 1
    };

    public CellByPartNo MapCellByPartNo(DbDataReader reader) => new()
    {
        PartNo = reader.GetString(reader.GetOrdinal("PartNo")),
        CellId = reader.GetInt32(reader.GetOrdinal("CellId")),
        CellName = reader.GetNullableString(reader.GetOrdinal("CellName"))
    };

    public CellByPartNoView MapCellByPartNoView(DbDataReader reader) => new()
    {
        PartNo = reader.GetString(reader.GetOrdinal("PartNo")),
        Family = reader.GetNullableString(reader.GetOrdinal("Family")),
        Subfamily = reader.GetNullableString(reader.GetOrdinal("Subfamily")),
        Description = reader.GetNullableString(reader.GetOrdinal("Description")),
        CellId = reader.GetInt32(reader.GetOrdinal("CellId")),
        CellName = reader.GetNullableString(reader.GetOrdinal("CellName"))
    };
}
