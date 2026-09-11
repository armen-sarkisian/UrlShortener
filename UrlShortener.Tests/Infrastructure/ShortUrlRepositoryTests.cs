using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Repositories;

namespace UrlShortener.Tests.Infrastructure;

/// <summary>
/// Репозиторий проверяется на настоящей базе (SQLite в памяти), а не на подменённом
/// провайдере: иначе уникальные индексы и порядок сортировки остались бы непроверенными.
/// </summary>
public sealed class ShortUrlRepositoryTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private AppDbContext _context = null!;
    private ShortUrlRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        _context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options);

        await _context.Database.EnsureCreatedAsync();

        _context.Users.Add(new ApplicationUser { Id = "user-1", UserName = "user", Email = "user@test.local" });
        await _context.SaveChangesAsync();

        _repository = new ShortUrlRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task SaveChangesAsync_TranslatesUniqueIndexViolationIntoDomainException()
    {
        await AddAsync("https://example.com", "4C92");

        await _repository.AddAsync(new ShortUrl
        {
            OriginalUrl = "https://example.com",
            Code = "4C93",
            CreatedById = "user-1",
            CreatedAtUtc = DateTime.UtcNow,
        });

        var exception = await Assert.ThrowsAsync<DuplicateUrlException>(() => _repository.SaveChangesAsync());
        Assert.Equal("https://example.com", exception.NormalizedUrl);
    }

    [Fact]
    public async Task SaveChangesAsync_LeavesContextUsableAfterRejectedInsert()
    {
        await AddAsync("https://example.com", "4C92");

        await _repository.AddAsync(new ShortUrl
        {
            OriginalUrl = "https://example.com",
            Code = "4C93",
            CreatedById = "user-1",
            CreatedAtUtc = DateTime.UtcNow,
        });
        await Assert.ThrowsAsync<DuplicateUrlException>(() => _repository.SaveChangesAsync());

        // Отклонённая запись не должна остаться в трекере и утащить за собой следующую вставку.
        await AddAsync("https://another.example.com", "4C94");

        Assert.Equal(2, (await _repository.GetAllAsync()).Count);
    }

    [Fact]
    public async Task ExistsAsync_FindsStoredAddress()
    {
        await AddAsync("https://example.com", "4C92");

        Assert.True(await _repository.ExistsAsync("https://example.com"));
        Assert.False(await _repository.ExistsAsync("https://example.com/other"));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsNewestFirstWithAuthor()
    {
        await AddAsync("https://older.example.com", "4C92", DateTime.UtcNow.AddHours(-1));
        await AddAsync("https://newer.example.com", "4C93", DateTime.UtcNow);

        var all = await _repository.GetAllAsync();

        Assert.Equal(["https://newer.example.com", "https://older.example.com"], all.Select(x => x.OriginalUrl));
        Assert.Equal("user", all[0].CreatedBy!.UserName);
    }

    [Fact]
    public async Task GetByCodeAsync_FindsRecordByShortCode()
    {
        await AddAsync("https://example.com", "4C92");

        Assert.Equal("https://example.com", (await _repository.GetByCodeAsync("4C92"))!.OriginalUrl);
        Assert.Null(await _repository.GetByCodeAsync("missing"));
    }

    private async Task AddAsync(string url, string code, DateTime? createdAt = null)
    {
        await _repository.AddAsync(new ShortUrl
        {
            OriginalUrl = url,
            Code = code,
            CreatedById = "user-1",
            CreatedAtUtc = createdAt ?? DateTime.UtcNow,
        });

        await _repository.SaveChangesAsync();
    }
}
