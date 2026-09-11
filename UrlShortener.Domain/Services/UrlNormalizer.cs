namespace UrlShortener.Domain.Services;

/// <summary>
/// Brings an address to its canonical form. Without this step the unique index is useless:
/// "HTTP://Site.com:80/" and "http://site.com" are the same resource but different strings.
/// </summary>
public static class UrlNormalizer
{
    private const int MaxLength = 2048;

    public static bool TryNormalize(string? input, out string normalized, out string? error)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "The address cannot be empty.";
            return false;
        }

        var candidate = input.Trim();

        if (candidate.Length > MaxLength)
        {
            error = $"The address is longer than {MaxLength} characters.";
            return false;
        }

        // People usually type "example.com" without a scheme — complete it into an absolute address.
        if (!candidate.Contains("://", StringComparison.Ordinal))
        {
            candidate = "https://" + candidate;
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            error = "The address could not be parsed.";
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            error = "Only http and https addresses are supported.";
            return false;
        }

        if (string.IsNullOrEmpty(uri.Host) || !uri.Host.Contains('.'))
        {
            error = "The address must contain a domain name.";
            return false;
        }

        var builder = new UriBuilder(uri)
        {
            Scheme = uri.Scheme.ToLowerInvariant(),
            Host = uri.Host.ToLowerInvariant(),
            // -1 makes UriBuilder omit the port when it matches the scheme default.
            Port = uri.IsDefaultPort ? -1 : uri.Port,
        };

        var result = builder.Uri.ToString();

        // A trailing slash only matters inside the path; at the root it is noise.
        if (builder.Uri.AbsolutePath == "/" && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment))
        {
            result = result.TrimEnd('/');
        }

        normalized = result;
        error = null;
        return true;
    }
}
