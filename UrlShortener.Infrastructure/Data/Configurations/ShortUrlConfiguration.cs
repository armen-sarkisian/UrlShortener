using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Data.Converters;

namespace UrlShortener.Infrastructure.Data.Configurations;

public sealed class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
{
    public void Configure(EntityTypeBuilder<ShortUrl> builder)
    {
        builder.Property(x => x.OriginalUrl).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(16);
        builder.Property(x => x.CreatedById).IsRequired();

        builder.Property(x => x.CreatedAtUtc).HasConversion<UtcDateTimeConverter>();
        builder.Property(x => x.LastAccessedAtUtc).HasConversion<UtcDateTimeConverter>();

        // The "URLs should be unique" requirement is held by an index, not only by a check in code.
        builder.HasIndex(x => x.OriginalUrl).IsUnique();
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne(x => x.CreatedBy)
            .WithMany(x => x.ShortUrls)
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
