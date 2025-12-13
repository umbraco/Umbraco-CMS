using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

[TestFixture]
public class Utf8ToAsciiConverterNewTests
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

        _converter = new Utf8ToAsciiConverterNew(loader);
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
}
