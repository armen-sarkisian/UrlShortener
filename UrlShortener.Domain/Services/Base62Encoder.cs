using System.Text;

namespace UrlShortener.Domain.Services;

/// <summary>
/// Кодирование целого числа в строку алфавитом из 62 символов.
/// Коды получаются короткими и, что важнее, уникальными без единой проверки в БД:
/// уникальность обеспечивает источник чисел, а кодирование её сохраняет (биекция).
/// </summary>
public static class Base62Encoder
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const int Base = 62;

    public static string Encode(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        if (value == 0)
        {
            return Alphabet[0].ToString();
        }

        // Максимум для long — 11 символов, поэтому буфера с запасом хватает всегда.
        var buffer = new StringBuilder(11);

        while (value > 0)
        {
            buffer.Append(Alphabet[(int)(value % Base)]);
            value /= Base;
        }

        // Разряды накапливались от младшего к старшему — возвращаем в привычном порядке.
        return string.Create(buffer.Length, buffer, static (span, source) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = source[source.Length - 1 - i];
            }
        });
    }

    public static long Decode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        long value = 0;

        foreach (var symbol in code)
        {
            var digit = Alphabet.IndexOf(symbol);

            if (digit < 0)
            {
                throw new FormatException($"Символ '{symbol}' не входит в алфавит Base62.");
            }

            value = checked(value * Base + digit);
        }

        return value;
    }
}
