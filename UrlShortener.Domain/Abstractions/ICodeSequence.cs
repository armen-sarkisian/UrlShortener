namespace UrlShortener.Domain.Abstractions;

/// <summary>
/// Источник монотонно возрастающих чисел для кодов. Вынесен за интерфейс,
/// потому что реализация упирается в конкретную СУБД (последовательность SQL Server),
/// а логика сокращения от этого зависеть не должна.
/// </summary>
public interface ICodeSequence
{
    Task<long> NextAsync(CancellationToken cancellationToken = default);
}
