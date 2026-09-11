namespace UrlShortener.Domain.Services;

/// <summary>
/// Приведение адреса к каноничному виду. Без этого шага уникальный индекс бесполезен:
/// "HTTP://Site.com:80/" и "http://site.com" — один и тот же ресурс, но разные строки.
/// </summary>
public static class UrlNormalizer
{
    private const int MaxLength = 2048;

    public static bool TryNormalize(string? input, out string normalized, out string? error)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Адрес не может быть пустым.";
            return false;
        }

        var candidate = input.Trim();

        if (candidate.Length > MaxLength)
        {
            error = $"Адрес длиннее {MaxLength} символов.";
            return false;
        }

        // Пользователи обычно пишут "example.com" без схемы — достраиваем до абсолютного адреса.
        if (!candidate.Contains("://", StringComparison.Ordinal))
        {
            candidate = "https://" + candidate;
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            error = "Не удалось разобрать адрес.";
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            error = "Поддерживаются только адреса http и https.";
            return false;
        }

        if (string.IsNullOrEmpty(uri.Host) || !uri.Host.Contains('.'))
        {
            error = "Адрес должен содержать доменное имя.";
            return false;
        }

        var builder = new UriBuilder(uri)
        {
            Scheme = uri.Scheme.ToLowerInvariant(),
            Host = uri.Host.ToLowerInvariant(),
            // -1 заставляет UriBuilder опустить порт, если он совпадает с портом схемы по умолчанию.
            Port = uri.IsDefaultPort ? -1 : uri.Port,
        };

        var result = builder.Uri.ToString();

        // Завершающий слэш значим только внутри пути; у корня он лишний.
        if (builder.Uri.AbsolutePath == "/" && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment))
        {
            result = result.TrimEnd('/');
        }

        normalized = result;
        error = null;
        return true;
    }
}
