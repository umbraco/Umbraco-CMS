using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

[TestFixture]
public class Utf8ToAsciiConverterTests
{
    private IUtf8ToAsciiConverter _converter = null!;

    [SetUp]
    public void SetUp()
    {
        var hostEnv = new Mock<IHostEnvironment>();
        hostEnv.Setup(h => h.ContentRootPath).Returns("/nonexistent");

        var loader = new CharacterMappingLoader(
            hostEnv.Object,
            NullLogger<CharacterMappingLoader>.Instance);

        _converter = new Utf8ToAsciiConverter(loader);
    }

    // === Null/Empty ===

    [Test]
    public void Convert_Null_ReturnsEmpty()
        => Assert.That(_converter.Convert(null), Is.EqualTo(string.Empty));

    [Test]
    public void Convert_Empty_ReturnsEmpty()
        => Assert.That(_converter.Convert(string.Empty), Is.EqualTo(string.Empty));

    // === ASCII Fast Path ===

    [TestCase("hello world", "hello world")]
    [TestCase("ABC123", "ABC123")]
    [TestCase("The quick brown fox", "The quick brown fox")]
    public void Convert_AsciiOnly_ReturnsSameString(string input, string expected)
        => Assert.That(_converter.Convert(input), Is.EqualTo(expected));

    // === Normalization (Accented Characters) ===

    [TestCase("café", "cafe")]
    [TestCase("naïve", "naive")]
    [TestCase("résumé", "resume")]
    public void Convert_AccentedChars_NormalizesCorrectly(string input, string expected)
        => Assert.That(_converter.Convert(input), Is.EqualTo(expected));

    // === Ligatures ===

    [TestCase("Œuvre", "OEuvre")]
    [TestCase("Ærodynamic", "AErodynamic")]
    [TestCase("straße", "strasse")]
    public void Convert_Ligatures_ExpandsCorrectly(string input, string expected)
        => Assert.That(_converter.Convert(input), Is.EqualTo(expected));

    // === Cyrillic ===
    // Note: These match the original Utf8ToAsciiConverter behavior (non-standard transliteration)

    [TestCase("Москва", "Moskva")]
    [TestCase("Борщ", "Borsh")]      // Original uses Щ→Sh (non-standard)
    [TestCase("Щука", "Shuka")]      // Original uses Щ→Sh (non-standard)
    [TestCase("Привет", "Privet")]
    public void Convert_Cyrillic_TransliteratesCorrectly(string input, string expected)
        => Assert.That(_converter.Convert(input), Is.EqualTo(expected));

    // === Special Latin ===

    [TestCase("Łódź", "Lodz")]
    [TestCase("Ørsted", "Orsted")]
    [TestCase("Þórr", "THorr")]
    public void Convert_SpecialLatin_ConvertsCorrectly(string input, string expected)
        => Assert.That(_converter.Convert(input), Is.EqualTo(expected));

    // === Span API ===

    [Test]
    public void Convert_SpanApi_WritesToOutputBuffer()
    {
        ReadOnlySpan<char> input = "café";
        Span<char> output = stackalloc char[20];

        var written = _converter.Convert(input, output);

        Assert.That(written, Is.EqualTo(4));
        Assert.That(new string(output[..written]), Is.EqualTo("cafe"));
    }

    [Test]
    public void Convert_SpanApi_HandlesExpansion()
    {
        ReadOnlySpan<char> input = "Щ"; // Expands to "Sh" (2 chars) in original
        Span<char> output = stackalloc char[20];

        var written = _converter.Convert(input, output);

        Assert.That(written, Is.EqualTo(2));
        Assert.That(new string(output[..written]), Is.EqualTo("Sh"));
    }

    // === Mixed Content ===

    [Test]
    public void Convert_MixedContent_HandlesCorrectly()
    {
        var input = "Café Müller in Moskva";
        var expected = "Cafe Muller in Moskva";

        Assert.That(_converter.Convert(input), Is.EqualTo(expected));
    }

    // === Edge Cases: Control Characters ===

    [Test]
    public void Convert_ControlCharacters_AreStripped()
    {
        // Tab, newline, carriage return should be stripped
        var input = "hello\t\n\rworld";
        var result = _converter.Convert(input);

        // Control characters are stripped (not converted to space)
        Assert.That(result, Is.EqualTo("helloworld"));
    }

    [Test]
    public void Convert_NullCharacter_IsStripped()
    {
        var input = "hello\0world";
        var result = _converter.Convert(input);

        Assert.That(result, Is.EqualTo("helloworld"));
    }

    // === Edge Cases: Whitespace Variants ===

    [Test]
    public void Convert_NonBreakingSpace_NormalizesToSpace()
    {
        // Non-breaking space (U+00A0)
        var input = "hello\u00A0world";
        var result = _converter.Convert(input);

        Assert.That(result, Is.EqualTo("hello world"));
    }

    [Test]
    public void Convert_EmSpace_NormalizesToSpace()
    {
        // Em space (U+2003)
        var input = "hello\u2003world";
        var result = _converter.Convert(input);

        Assert.That(result, Is.EqualTo("hello world"));
    }

    // === Edge Cases: Empty Mappings ===

    [Test]
    public void Convert_CyrillicHardSign_MapsToQuote()
    {
        // Ъ maps to " in original Umbraco implementation
        var input = "Ъ";
        var result = _converter.Convert(input);

        Assert.That(result, Is.EqualTo("\""));
    }

    [Test]
    public void Convert_CyrillicSoftSign_MapsToApostrophe()
    {
        // Ь maps to ' in original Umbraco implementation
        var input = "Ь";
        var result = _converter.Convert(input);

        Assert.That(result, Is.EqualTo("'"));
    }

    // === Edge Cases: Surrogate Pairs (Emoji) ===

    [Test]
    public void Convert_Emoji_ReplacedWithFallback()
    {
        // Emoji (surrogate pair)
        var input = "hello 😀 world";
        var result = _converter.Convert(input);

        Assert.That(result, Is.EqualTo("hello ? world"));
    }

    [Test]
    public void Convert_Emoji_CustomFallback()
    {
        var input = "test 🎉 emoji";
        var result = _converter.Convert(input, fallback: '*');

        Assert.That(result, Is.EqualTo("test * emoji"));
    }

    // === Edge Cases: Long Input ===

    [Test]
    public void Convert_LongAsciiString_ReturnsSameReference()
    {
        // Pure ASCII should return same string instance (no allocation)
        var input = new string('a', 10000);
        var result = _converter.Convert(input);

        Assert.That(ReferenceEquals(input, result), Is.True,
            "Pure ASCII input should return same string instance");
    }
}
