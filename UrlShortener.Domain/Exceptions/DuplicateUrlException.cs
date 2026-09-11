namespace UrlShortener.Domain.Exceptions;

/// <summary>
/// The unique index rejected the insert. Checking whether the address already exists does not
/// protect against a race between two concurrent requests, so the database has the final say.
/// </summary>
public sealed class DuplicateUrlException(string normalizedUrl)
    : Exception($"Address '{normalizedUrl}' has already been shortened.")
{
    public string NormalizedUrl { get; } = normalizedUrl;
}
