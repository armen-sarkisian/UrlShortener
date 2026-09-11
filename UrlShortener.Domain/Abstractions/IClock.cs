namespace UrlShortener.Domain.Abstractions;

/// <summary>Current time behind an interface — otherwise creation dates are untestable.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
