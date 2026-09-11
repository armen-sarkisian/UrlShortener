using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Data;

/// <summary>
/// Applies migrations and creates the starting data: roles, a couple of accounts and the text of
/// the About page. It exists so the application comes up on a clean machine with a single command.
/// </summary>
public static class DatabaseSeeder
{
    public const string AdminLogin = "admin";
    public const string AdminPassword = "Admin123$";
    public const string UserLogin = "user";
    public const string UserPassword = "User123$";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync(cancellationToken);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await EnsureRoleAsync(roleManager, Roles.Admin);
        await EnsureRoleAsync(roleManager, Roles.User);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await EnsureUserAsync(userManager, AdminLogin, "admin@urlshortener.local", AdminPassword, Roles.Admin);
        await EnsureUserAsync(userManager, UserLogin, "user@urlshortener.local", UserPassword, Roles.User);

        await EnsureAboutPageAsync(context, cancellationToken);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string login,
        string email,
        string password,
        string role)
    {
        if (await userManager.FindByNameAsync(login) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = login,
            Email = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create user '{login}': {string.Join("; ", result.Errors.Select(x => x.Description))}");
        }

        await userManager.AddToRoleAsync(user, role);
    }

    private static async Task EnsureAboutPageAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.AboutPages.AnyAsync(cancellationToken))
        {
            return;
        }

        context.AboutPages.Add(new AboutPage
        {
            Id = AboutPage.SingletonId,
            Content = DefaultAboutContent,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    private const string DefaultAboutContent = """
        Shortening algorithm

        Every link is assigned a number from a SQL Server sequence (ShortUrlCodeSequence), which is
        then encoded in base 62 over the alphabet 0-9, A-Z, a-z. For example, 1000000 becomes "4C92".

        Why this way:
        - the sequence hands out values atomically, so codes are unique by construction — neither a
          retry loop nor a check for a taken code is needed;
        - base 62 encoding is a bijection, so uniqueness of the numbers carries over to the codes,
          while length grows logarithmically: 62^4 already covers more than 14 million addresses;
        - starting the sequence at 1,000,000 yields four-character codes right away and does not
          reveal how many links the system holds.

        The address itself is normalized before it is stored: scheme and host are lowercased, the
        default port and the trailing slash at the root are dropped. Uniqueness of the normalized
        address is enforced by a unique index in the database, so shortening the same link twice
        returns an error even under concurrent requests.

        A short link is followed at /s/{code}.
        """;
}
