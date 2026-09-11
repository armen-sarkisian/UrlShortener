namespace UrlShortener.Domain.Entities;

/// <summary>
/// A shortened link. <see cref="OriginalUrl"/> is stored in normalized form so that
/// the unique index actually catches duplicates.
/// </summary>
public class ShortUrl
{
    public int Id { get; set; }

    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>Base62 code the redirect runs on: <c>/s/{Code}</c>.</summary>
    public string Code { get; set; } = string.Empty;

    public string CreatedById { get; set; } = string.Empty;

    public ApplicationUser? CreatedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int ClickCount { get; set; }

    public DateTime? LastAccessedAtUtc { get; set; }

    /// <summary>A record may be deleted by its author or by an administrator.</summary>
    public bool CanBeDeletedBy(string userId, bool isAdmin) => isAdmin || CreatedById == userId;
}
