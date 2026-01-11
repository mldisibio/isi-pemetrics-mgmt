using System.Collections.Immutable;
using System.Data.Common;

namespace PEMetrics.DataApi.Infrastructure;

/// <summary>Extension methods for DbDataReader providing functional mapping patterns.</summary>
public static class DataReaderExtensions
{
    /// <summary>Maps all rows from the reader using the provided mapping function.</summary>
    public static ImmutableList<T> MapAll<T>(this DbDataReader reader, Func<DbDataReader, T> mapper)
    {
        var builder = ImmutableList.CreateBuilder<T>();
        while (reader.Read())
        {
            builder.Add(mapper(reader));
        }
        return builder.ToImmutable();
    }

    /// <summary>Maps a single row if present, otherwise returns default.</summary>
    public static T? MapFirstOrDefault<T>(this DbDataReader reader, Func<DbDataReader, T> mapper) where T : class
    {
        return reader.Read() ? mapper(reader) : null;
    }

    /// <summary>Gets a nullable string value from the reader.</summary>
    public static string? GetNullableString(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    /// <summary>Gets a nullable DateOnly value from the reader.</summary>
    public static DateOnly? GetNullableDateOnly(this DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : DateOnly.FromDateTime(reader.GetDateTime(ordinal));
    }

    /// <summary>Gets a DateOnly value from the reader.</summary>
    public static DateOnly GetDateOnly(this DbDataReader reader, int ordinal)
    {
        return DateOnly.FromDateTime(reader.GetDateTime(ordinal));
    }
}
