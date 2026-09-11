using NSubstitute;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.Models;
using UrlShortener.Domain.Services;

namespace UrlShortener.Tests.Domain;

public class ShortUrlServiceTests
{
    private static readonly DateTime Now = new(2026, 9, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly IShortUrlRepository _repository = Substitute.For<IShortUrlRepository>();
    private readonly ICodeSequence _codeSequence = Substitute.For<ICodeSequence>();
    private readonly ShortUrlService _service;

    public ShortUrlServiceTests()
    {
        _codeSequence.NextAsync().ReturnsForAnyArgs(1_000_000L);
        _service = new ShortUrlService(_repository, _codeSequence, new FixedClock(Now));
    }

    [Fact]
    public async Task CreateAsync_StoresNormalizedAddressWithGeneratedCode()
    {
        var result = await _service.CreateAsync("HTTPS://Example.COM:443/Docs/", "user-1");

        Assert.Equal(CreateStatus.Created, result.Status);
        Assert.Equal("https://example.com/Docs/", result.ShortUrl!.OriginalUrl);
        Assert.Equal("4C92", result.ShortUrl.Code);
        Assert.Equal("user-1", result.ShortUrl.CreatedById);
        Assert.Equal(Now, result.ShortUrl.CreatedAtUtc);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidAddressWithoutTouchingStorage()
    {
        var result = await _service.CreateAsync("ftp://example.com", "user-1");

        Assert.Equal(CreateStatus.InvalidUrl, result.Status);
        Assert.NotNull(result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<ShortUrl>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReportsDuplicateWhenAddressAlreadyShortened()
    {
        _repository.ExistsAsync("https://example.com", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.CreateAsync("https://example.com/", "user-1");

        Assert.Equal(CreateStatus.Duplicate, result.Status);
        await _repository.DidNotReceive().AddAsync(Arg.Any<ShortUrl>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReportsDuplicateWhenDatabaseRejectsConcurrentInsert()
    {
        // Гонка: проверка прошла, но параллельный запрос успел вставить тот же адрес раньше.
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException(new DuplicateUrlException("https://example.com")));

        var result = await _service.CreateAsync("https://example.com", "user-1");

        Assert.Equal(CreateStatus.Duplicate, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_AllowsAuthorToRemoveOwnRecord()
    {
        var shortUrl = Existing(id: 5, ownerId: "user-1");

        var status = await _service.DeleteAsync(5, "user-1", isAdmin: false);

        Assert.Equal(DeleteStatus.Deleted, status);
        _repository.Received(1).Remove(shortUrl);
    }

    [Fact]
    public async Task DeleteAsync_ForbidsRemovingSomeoneElsesRecord()
    {
        var shortUrl = Existing(id: 5, ownerId: "user-1");

        var status = await _service.DeleteAsync(5, "user-2", isAdmin: false);

        Assert.Equal(DeleteStatus.Forbidden, status);
        _repository.DidNotReceive().Remove(shortUrl);
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_AllowsAdminToRemoveAnyRecord()
    {
        var shortUrl = Existing(id: 5, ownerId: "user-1");

        var status = await _service.DeleteAsync(5, "admin-1", isAdmin: true);

        Assert.Equal(DeleteStatus.Deleted, status);
        _repository.Received(1).Remove(shortUrl);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFoundForUnknownId()
    {
        var status = await _service.DeleteAsync(42, "user-1", isAdmin: true);

        Assert.Equal(DeleteStatus.NotFound, status);
    }

    [Fact]
    public async Task ResolveAsync_ReturnsOriginalAddressAndCountsTransition()
    {
        var shortUrl = new ShortUrl { Id = 1, Code = "4C92", OriginalUrl = "https://example.com", ClickCount = 2 };
        _repository.GetByCodeAsync("4C92", Arg.Any<CancellationToken>()).Returns(shortUrl);

        var original = await _service.ResolveAsync("4C92");

        Assert.Equal("https://example.com", original);
        Assert.Equal(3, shortUrl.ClickCount);
        Assert.Equal(Now, shortUrl.LastAccessedAtUtc);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNullForUnknownCode()
    {
        Assert.Null(await _service.ResolveAsync("missing"));
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private ShortUrl Existing(int id, string ownerId)
    {
        var shortUrl = new ShortUrl
        {
            Id = id,
            Code = "4C92",
            OriginalUrl = "https://example.com",
            CreatedById = ownerId,
            CreatedAtUtc = Now,
        };

        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(shortUrl);

        return shortUrl;
    }

    private sealed class FixedClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
