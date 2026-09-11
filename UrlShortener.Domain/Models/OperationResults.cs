using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Models;

public enum CreateStatus
{
    Created,
    InvalidUrl,
    Duplicate,
}

public enum DeleteStatus
{
    Deleted,
    NotFound,
    Forbidden,
}

public sealed record CreateResult(CreateStatus Status, ShortUrl? ShortUrl, string? Error)
{
    public static CreateResult Created(ShortUrl shortUrl) => new(CreateStatus.Created, shortUrl, null);

    public static CreateResult Invalid(string error) => new(CreateStatus.InvalidUrl, null, error);

    public static CreateResult Duplicate(string error) => new(CreateStatus.Duplicate, null, error);
}
