using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Data.Converters;

namespace UrlShortener.Infrastructure.Data.Configurations;

public sealed class AboutPageConfiguration : IEntityTypeConfiguration<AboutPage>
{
    public void Configure(EntityTypeBuilder<AboutPage> builder)
    {
        // There is exactly one row and its identifier is fixed, so the database does not generate it.
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Content).IsRequired();

        builder.Property(x => x.UpdatedAtUtc).HasConversion<UtcDateTimeConverter>();

        builder.HasOne(x => x.UpdatedBy)
            .WithMany()
            .HasForeignKey(x => x.UpdatedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
