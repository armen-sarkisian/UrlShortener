namespace UrlShortener.Domain.Abstractions;

/// <summary>Текущее время за интерфейсом — иначе даты создания нечем проверить в тестах.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
