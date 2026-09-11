using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories;

public sealed class ShortUrlRepository(AppDbContext context) : IShortUrlRepository
{
    private const int SqlServerDuplicateKey = 2627;
    private const int SqlServerDuplicateIndex = 2601;

    public async Task<IReadOnlyList<ShortUrl>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.ShortUrls
            .AsNoTracking()
            .Include(x => x.CreatedBy)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<ShortUrl?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.ShortUrls
            .Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ShortUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        context.ShortUrls.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public Task<bool> ExistsAsync(string normalizedUrl, CancellationToken cancellationToken = default) =>
        context.ShortUrls.AnyAsync(x => x.OriginalUrl == normalizedUrl, cancellationToken);

    public async Task AddAsync(ShortUrl shortUrl, CancellationToken cancellationToken = default) =>
        await context.ShortUrls.AddAsync(shortUrl, cancellationToken);

    public void Remove(ShortUrl shortUrl) => context.ShortUrls.Remove(shortUrl);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            // A unique index violation is not a failure but an expected outcome: the address exists.
            // Translate it into a domain exception so the service stays unaware of provider specifics.
            var duplicate = context.ChangeTracker.Entries<ShortUrl>()
                .FirstOrDefault(x => x.State == EntityState.Added)?.Entity;

            context.ChangeTracker.Clear();

            throw new DuplicateUrlException(duplicate?.OriginalUrl ?? string.Empty);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) => exception.InnerException switch
    {
        SqlException sql => sql.Number is SqlServerDuplicateKey or SqlServerDuplicateIndex,
        // Providers other than SQL Server (SQLite in tests) have no exception type of their own here,
        // so we fall back to the message: EF Core exposes no portable indicator.
        { } other => other.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || other.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase),
        _ => false,
    };
}
