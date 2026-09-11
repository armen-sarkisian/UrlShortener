namespace UrlShortener.Domain.Entities;

/// <summary>Содержимое страницы About. В таблице всегда одна строка с <see cref="SingletonId"/>.</summary>
public class AboutPage
{
    public const int SingletonId = 1;

    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }

    public string? UpdatedById { get; set; }

    public ApplicationUser? UpdatedBy { get; set; }
}
