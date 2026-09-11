namespace UrlShortener.Domain.Entities;

/// <summary>
/// Сокращённая ссылка. <see cref="OriginalUrl"/> хранится в нормализованном виде,
/// чтобы уникальный индекс действительно ловил дубликаты.
/// </summary>
public class ShortUrl
{
    public int Id { get; set; }

    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>Base62-код, по которому работает переход: <c>/s/{Code}</c>.</summary>
    public string Code { get; set; } = string.Empty;

    public string CreatedById { get; set; } = string.Empty;

    public ApplicationUser? CreatedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int ClickCount { get; set; }

    public DateTime? LastAccessedAtUtc { get; set; }

    /// <summary>Удалять запись может её автор либо администратор.</summary>
    public bool CanBeDeletedBy(string userId, bool isAdmin) => isAdmin || CreatedById == userId;
}
