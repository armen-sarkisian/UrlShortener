using System.Text;

namespace UrlShortener.Domain.Services;

/// <summary>
/// Encodes an integer into a string over a 62-symbol alphabet.
/// Codes stay short and, more importantly, unique without a single database check:
/// uniqueness comes from the source of numbers, and the encoding preserves it (it is a bijection).
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

        // A long never exceeds 11 symbols, so this capacity is always enough.
        var buffer = new StringBuilder(11);

        while (value > 0)
        {
            buffer.Append(Alphabet[(int)(value % Base)]);
            value /= Base;
        }

        // Digits were produced from the least significant one — return them in the usual order.
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
                throw new FormatException($"Symbol '{symbol}' is not part of the Base62 alphabet.");
            }

            value = checked(value * Base + digit);
        }

        return value;
    }
}
