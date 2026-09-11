using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Domain.Entities;

/// <summary>Пользователь приложения. Роли (<see cref="Roles"/>) хранятся средствами Identity.</summary>
public class ApplicationUser : IdentityUser
{
    public ICollection<ShortUrl> ShortUrls { get; set; } = new List<ShortUrl>();
}
