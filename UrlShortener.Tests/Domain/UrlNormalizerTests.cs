using UrlShortener.Domain.Services;

namespace UrlShortener.Tests.Domain;

public class UrlNormalizerTests
{
    [Theory]
    [InlineData("https://example.com", "https://example.com")]
    [InlineData("https://example.com/", "https://example.com")]
    [InlineData("HTTPS://Example.COM/", "https://example.com")]
    [InlineData("https://example.com:443/docs", "https://example.com/docs")]
    [InlineData("http://example.com:80/docs", "http://example.com/docs")]
    [InlineData("  https://example.com/docs  ", "https://example.com/docs")]
    [InlineData("example.com/docs", "https://example.com/docs")]
    public void TryNormalize_BringsEquivalentAddressesToSameForm(string input, string expected)
    {
        Assert.True(UrlNormalizer.TryNormalize(input, out var normalized, out _));
        Assert.Equal(expected, normalized);
    }

    [Fact]
    public void TryNormalize_KeepsPathCaseIntact()
    {
        // Хост регистронезависим, а путь — нет: /Docs и /docs могут быть разными страницами.
        Assert.True(UrlNormalizer.TryNormalize("https://EXAMPLE.com/Docs", out var normalized, out _));
        Assert.Equal("https://example.com/Docs", normalized);
    }

    [Fact]
    public void TryNormalize_KeepsQueryString()
    {
        Assert.True(UrlNormalizer.TryNormalize("https://example.com/search?q=1", out var normalized, out _));
        Assert.Equal("https://example.com/search?q=1", normalized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("localhost")]
    public void TryNormalize_RejectsUnsupportedInput(string? input)
    {
        Assert.False(UrlNormalizer.TryNormalize(input, out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void TryNormalize_RejectsTooLongAddress()
    {
        var tooLong = "https://example.com/" + new string('a', 2100);

        Assert.False(UrlNormalizer.TryNormalize(tooLong, out _, out var error));
        Assert.Contains("2048", error);
    }
}
