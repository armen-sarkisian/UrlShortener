using UrlShortener.Domain.Services;

namespace UrlShortener.Tests.Domain;

public class Base62EncoderTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(10, "A")]
    [InlineData(61, "z")]
    [InlineData(62, "10")]
    [InlineData(1_000_000, "4C92")]
    public void Encode_ReturnsExpectedCode(long value, string expected) =>
        Assert.Equal(expected, Base62Encoder.Encode(value));

    [Theory]
    [InlineData(0)]
    [InlineData(61)]
    [InlineData(62)]
    [InlineData(1_000_000)]
    [InlineData(long.MaxValue)]
    public void EncodeThenDecode_ReturnsOriginalValue(long value) =>
        Assert.Equal(value, Base62Encoder.Decode(Base62Encoder.Encode(value)));

    [Fact]
    public void Encode_ProducesDistinctCodesForDistinctValues()
    {
        // Code uniqueness rests precisely on the encoding being a bijection,
        // so it is checked explicitly over a continuous range.
        var codes = Enumerable.Range(1_000_000, 5_000).Select(x => Base62Encoder.Encode(x)).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void Encode_KeepsCodeShort()
    {
        Assert.Equal(4, Base62Encoder.Encode(1_000_000).Length);
        Assert.Equal(6, Base62Encoder.Encode(62L * 62 * 62 * 62 * 62).Length);
    }

    [Fact]
    public void Encode_ThrowsOnNegativeValue() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Base62Encoder.Encode(-1));

    [Fact]
    public void Decode_ThrowsOnSymbolOutsideAlphabet() =>
        Assert.Throws<FormatException>(() => Base62Encoder.Decode("abc-def"));
}
