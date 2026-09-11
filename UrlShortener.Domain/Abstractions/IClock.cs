namespace UrlShortener.Domain.Abstractions;

/// <summary>Current time behind an interface, so creation dates stay substitutable.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
