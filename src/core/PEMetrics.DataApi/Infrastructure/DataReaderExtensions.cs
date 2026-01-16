using System.Collections.Immutable;
using System.Data.Common;

namespace PEMetrics.DataApi.Infrastructure;

/// <summary>Extension methods for DbDataReader providing functional mapping patterns.</summary>
public static class DataReaderExtensions
{
    /// <summary>Maps all rows from the reader using the provided mapping function.</summary>
    public static async Task<ImmutableList<T>> MapAllAsync<T>(this DbDataReader reader, Func<DbDataReader, T> mapper, CancellationToken cancellationToken = default)
    {
        var builder = ImmutableList.CreateBuilder<T>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            builder.Add(mapper(reader));
        }
        return builder.ToImmutable();
    }

    /// <summary>Maps a single row if present, otherwise returns default.</summary>
    public static async Task<T?> MapFirstOrDefaultAsync<T>(this DbDataReader reader, Func<DbDataReader, T> mapper, CancellationToken cancellationToken = default) where T : class
    {
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? mapper(reader) : null;
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
