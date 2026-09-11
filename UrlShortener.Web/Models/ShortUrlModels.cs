using UrlShortener.Domain.Entities;

namespace UrlShortener.Web.Models;

/// <summary>Representation of a link for Angular and for the details page.</summary>
public sealed record ShortUrlDto(
    int Id,
    string OriginalUrl,
    string Code,
    string ShortUrl,
    string CreatedBy,
    DateTime CreatedAtUtc,
    int ClickCount,
    DateTime? LastAccessedAtUtc,
    bool CanDelete)
{
    public static ShortUrlDto From(ShortUrl source, string baseUrl, string? currentUserId, bool isAdmin) => new(
        source.Id,
        source.OriginalUrl,
        source.Code,
        $"{baseUrl}/s/{source.Code}",
        source.CreatedBy?.UserName ?? "—",
        source.CreatedAtUtc,
        source.ClickCount,
        source.LastAccessedAtUtc,
        currentUserId is not null && source.CanBeDeletedBy(currentUserId, isAdmin));
}

public sealed class CreateShortUrlRequest
{
    public string? Url { get; set; }
}

/// <summary>Current user context — Angular decides what to show based on it.</summary>
public sealed record SessionDto(bool IsAuthenticated, string? UserName, bool IsAdmin, string AntiforgeryToken);
