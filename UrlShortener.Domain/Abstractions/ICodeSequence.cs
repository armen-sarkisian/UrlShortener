namespace UrlShortener.Domain.Abstractions;

/// <summary>
/// Source of monotonically increasing numbers for short codes. It sits behind an interface
/// because the implementation is tied to a specific database (a SQL Server sequence),
/// and the shortening logic must not depend on that.
/// </summary>
public interface ICodeSequence
{
    Task<long> NextAsync(CancellationToken cancellationToken = default);
}
