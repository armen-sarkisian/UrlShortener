using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>Последовательность, из которой берутся числа для Base62-кодов.</summary>
    public const string CodeSequenceName = "ShortUrlCodeSequence";

    /// <summary>
    /// Старт не с единицы: так первые коды сразу четырёхсимвольные ("4c92"),
    /// а не "1", "2", "3" — и заодно не выдают, сколько ссылок в системе.
    /// </summary>
    public const long CodeSequenceStart = 1_000_000;

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    public DbSet<AboutPage> AboutPages => Set<AboutPage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Последовательности есть не у всех провайдеров: в тестах модель поднимается на SQLite,
        // который их не поддерживает, а код там выдаёт подставная реализация ICodeSequence.
        if (Database.IsSqlServer())
        {
            builder.HasSequence<long>(CodeSequenceName).StartsAt(CodeSequenceStart).IncrementsBy(1);
        }

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
