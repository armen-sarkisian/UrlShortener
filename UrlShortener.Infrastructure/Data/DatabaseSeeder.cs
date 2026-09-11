using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Data;

/// <summary>
/// Накатывает миграции и создаёт стартовые данные: роли, пару учётных записей
/// и текст страницы About. Нужен, чтобы приложение поднималось на чистой машине одной командой.
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
                $"Не удалось создать пользователя '{login}': {string.Join("; ", result.Errors.Select(x => x.Description))}");
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
        Алгоритм сокращения

        Каждой ссылке присваивается число из последовательности SQL Server
        (ShortUrlCodeSequence), которое затем кодируется в системе счисления по основанию 62
        алфавитом 0-9, A-Z, a-z. Например, 1000000 превращается в "4c92".

        Почему именно так:
        - последовательность выдаёт значения атомарно, поэтому коды уникальны по построению —
          не нужен ни цикл повторных попыток, ни проверка занятости кода в базе;
        - кодирование по основанию 62 — биекция, значит уникальность чисел переносится на коды,
          а длина растёт логарифмически: 62^4 — это уже более 14 миллионов адресов;
        - старт последовательности с 1 000 000 даёт коды сразу из четырёх символов и не раскрывает,
          сколько ссылок в системе.

        Сам адрес перед сохранением нормализуется: схема и хост приводятся к нижнему регистру,
        отбрасывается порт по умолчанию и завершающий слэш у корня. Уникальность нормализованного
        адреса обеспечивает уникальный индекс в базе, поэтому повторное сокращение одной и той же
        ссылки возвращает ошибку даже при одновременных запросах.

        Переход по короткой ссылке выполняется по адресу /s/{код}.
        """;
}
