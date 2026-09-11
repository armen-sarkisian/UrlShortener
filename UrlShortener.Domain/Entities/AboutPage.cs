namespace UrlShortener.Domain.Entities;

/// <summary>Content of the About page. The table always holds a single row with <see cref="SingletonId"/>.</summary>
public class AboutPage
{
    public const int SingletonId = 1;

    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }

    public string? UpdatedById { get; set; }

    public ApplicationUser? UpdatedBy { get; set; }
}
