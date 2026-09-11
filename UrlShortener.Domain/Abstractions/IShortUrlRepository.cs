using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Abstractions;

public interface IShortUrlRepository
{
    Task<IReadOnlyList<ShortUrl>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ShortUrl?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ShortUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string normalizedUrl, CancellationToken cancellationToken = default);

    Task AddAsync(ShortUrl shortUrl, CancellationToken cancellationToken = default);

    void Remove(ShortUrl shortUrl);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
