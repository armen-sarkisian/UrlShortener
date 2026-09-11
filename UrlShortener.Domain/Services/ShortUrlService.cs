using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.Models;

namespace UrlShortener.Domain.Services;

public interface IShortUrlService
{
    Task<IReadOnlyList<ShortUrl>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ShortUrl?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<CreateResult> CreateAsync(string? rawUrl, string userId, CancellationToken cancellationToken = default);

    Task<DeleteStatus> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken cancellationToken = default);

    Task<string?> ResolveAsync(string code, CancellationToken cancellationToken = default);
}

public sealed class ShortUrlService(
    IShortUrlRepository repository,
    ICodeSequence codeSequence,
    IClock clock) : IShortUrlService
{
    public Task<IReadOnlyList<ShortUrl>> GetAllAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public Task<ShortUrl?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<CreateResult> CreateAsync(
        string? rawUrl,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (!UrlNormalizer.TryNormalize(rawUrl, out var normalized, out var error))
        {
            return CreateResult.Invalid(error!);
        }

        if (await repository.ExistsAsync(normalized, cancellationToken))
        {
            return CreateResult.Duplicate($"Адрес уже сокращён: {normalized}");
        }

        var shortUrl = new ShortUrl
        {
            OriginalUrl = normalized,
            Code = Base62Encoder.Encode(await codeSequence.NextAsync(cancellationToken)),
            CreatedById = userId,
            CreatedAtUtc = clock.UtcNow,
        };

        try
        {
            await repository.AddAsync(shortUrl, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (DuplicateUrlException)
        {
            return CreateResult.Duplicate($"Адрес уже сокращён: {normalized}");
        }

        // Перечитываем запись: в только что созданной сущности навигация на автора пуста,
        // а клиенту нужен тот же набор полей, что и при обычном чтении списка.
        return CreateResult.Created(await repository.GetByIdAsync(shortUrl.Id, cancellationToken) ?? shortUrl);
    }

    public async Task<DeleteStatus> DeleteAsync(
        int id,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var shortUrl = await repository.GetByIdAsync(id, cancellationToken);

        if (shortUrl is null)
        {
            return DeleteStatus.NotFound;
        }

        if (!shortUrl.CanBeDeletedBy(userId, isAdmin))
        {
            return DeleteStatus.Forbidden;
        }

        repository.Remove(shortUrl);
        await repository.SaveChangesAsync(cancellationToken);

        return DeleteStatus.Deleted;
    }

    public async Task<string?> ResolveAsync(string code, CancellationToken cancellationToken = default)
    {
        var shortUrl = await repository.GetByCodeAsync(code, cancellationToken);

        if (shortUrl is null)
        {
            return null;
        }

        shortUrl.ClickCount++;
        shortUrl.LastAccessedAtUtc = clock.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);

        return shortUrl.OriginalUrl;
    }
}
