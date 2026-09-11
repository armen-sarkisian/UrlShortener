using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace UrlShortener.Infrastructure.Data.Converters;

/// <summary>
/// SQL Server возвращает datetime2 без сведений о часовом поясе, и прочитанное значение
/// приходит с DateTimeKind.Unspecified. Из-за этого одна и та же дата сериализуется
/// то с суффиксом Z (когда объект ещё в памяти), то без него — клиент видит разные форматы.
/// Конвертер фиксирует: в этих колонках всегда UTC.
/// </summary>
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value,
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
