using UrlShortener.Domain.Entities;

namespace UrlShortener.Web.Models;

/// <summary>Представление ссылки для Angular и страницы деталей.</summary>
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

/// <summary>Контекст текущего пользователя — по нему Angular решает, что показывать.</summary>
public sealed record SessionDto(bool IsAuthenticated, string? UserName, bool IsAdmin, string AntiforgeryToken);
