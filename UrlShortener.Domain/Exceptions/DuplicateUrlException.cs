namespace UrlShortener.Domain.Exceptions;

/// <summary>
/// Уникальный индекс отверг вставку. Проверка "существует ли уже такой адрес" не спасает
/// от гонки между двумя одновременными запросами, поэтому последнее слово остаётся за БД.
/// </summary>
public sealed class DuplicateUrlException(string normalizedUrl)
    : Exception($"Адрес '{normalizedUrl}' уже сокращён.")
{
    public string NormalizedUrl { get; } = normalizedUrl;
}
