using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Domain.Entities;

/// <summary>Application user. Roles (see <see cref="Roles"/>) are stored by Identity.</summary>
public class ApplicationUser : IdentityUser
{
    public ICollection<ShortUrl> ShortUrls { get; set; } = new List<ShortUrl>();
}
