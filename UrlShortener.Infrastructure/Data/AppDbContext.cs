using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>Sequence the numbers behind Base62 codes are taken from.</summary>
    public const string CodeSequenceName = "ShortUrlCodeSequence";

    /// <summary>
    /// Not starting at one: this way the first codes are already four symbols long ("4C92")
    /// instead of "1", "2", "3", and they do not reveal how many links the system holds.
    /// </summary>
    public const long CodeSequenceStart = 1_000_000;

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    public DbSet<AboutPage> AboutPages => Set<AboutPage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Not every provider has sequences: tests build the model on SQLite, which has none,
        // and there the codes come from a stubbed ICodeSequence implementation.
        if (Database.IsSqlServer())
        {
            builder.HasSequence<long>(CodeSequenceName).StartsAt(CodeSequenceStart).IncrementsBy(1);
        }

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
