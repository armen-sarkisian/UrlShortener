using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace UrlShortener.Infrastructure.Data.Converters;

/// <summary>
/// SQL Server returns datetime2 without time zone information, so a value read back carries
/// DateTimeKind.Unspecified. As a result the same date is serialized sometimes with a "Z" suffix
/// (while the object is still in memory) and sometimes without it, and the client sees two formats.
/// This converter states it once: these columns always hold UTC.
/// </summary>
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value,
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
