using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Tests for the <see cref="LottieFilesEmbedProvider"/> class.
/// </summary>
[TestFixture]
public class LottieFilesEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new LottieFiles(Mock.Of<IJsonSerializer>());

/// <summary>
/// Tests that valid LottieFiles URLs are matched by the provider's URL scheme regex.
/// </summary>
/// <param name="url">The URL to test against the regex.</param>
    [TestCase("https://www.lottiefiles.com/animations/abc123")]
    [TestCase("https://lottiefiles.com/animations/abc123")]
    [TestCase("http://www.lottiefiles.com/animations/abc123")]
    [TestCase("http://lottiefiles.com/animations/abc123")]
    public void UrlSchemeRegex_MatchesValidLottieFilesUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

/// <summary>
/// Tests that URLs with LottieFiles domain in the path (potential SSRF vector) are NOT matched.
/// </summary>
/// <param name="url">The URL to test against the regex.</param>
    [TestCase("http://127.0.0.1/lottiefiles.com/animations/abc123")]
    [TestCase("http://localhost/lottiefiles.com/animations/abc123")]
    [TestCase("http://example.com/lottiefiles.com/animations/abc123")]
    [TestCase("http://example.com/redirect?url=https://lottiefiles.com/animations/abc123")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that unrelated URLs are not matched.
    /// </summary>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notlottiefiles.com/animations/abc123")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }

    /// <summary>
    /// Tests that BuildMarkup returns null when passed null HTML.
    /// </summary>
    [Test]
    public void BuildMarkup_WithNullHtml_ReturnsNull()
    {
        // Act
        var result = LottieFiles.BuildMarkup(null, 800, 600);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests that BuildMarkup correctly replaces the width and height attributes with valid dimensions.
    /// </summary>
    [Test]
    public void BuildMarkup_WithValidDimensions_ReplacesDimensions()
    {
        // Arrange
        var html = """<iframe src="https://embed.lottiefiles.com/animation/12345" width="300" height="200" frameborder="0"></iframe>""";

        // Act
        var result = LottieFiles.BuildMarkup(html, 800, 600);

        // Assert
        Assert.That(result, Does.Contain("width=\"800\""));
        Assert.That(result, Does.Contain("height=\"600\""));
    }

    /// <summary>
    /// Tests that when zero dimensions are provided, the markup defaults to 100% width and height.
    /// </summary>
    [Test]
    public void BuildMarkup_WithZeroDimensions_DefaultsTo100Percent()
    {
        // Arrange
        var html = """<iframe src="https://embed.lottiefiles.com/animation/12345" width="300" height="200" frameborder="0"></iframe>""";

        // Act
        var result = LottieFiles.BuildMarkup(html, 0, 0);

        // Assert
        Assert.That(result, Does.Contain("width=\"100%\""));
        Assert.That(result, Does.Contain("height=\"100%\""));
    }

    /// <summary>
    /// Tests that when null dimensions are provided, the markup defaults to 100% width and height.
    /// </summary>
    [Test]
    public void BuildMarkup_WithNullDimensions_DefaultsTo100Percent()
    {
        // Arrange
        var html = """<iframe src="https://embed.lottiefiles.com/animation/12345" width="300" height="200" frameborder="0"></iframe>""";

        // Act
        var result = LottieFiles.BuildMarkup(html, null, null);

        // Assert
        Assert.That(result, Does.Contain("width=\"100%\""));
        Assert.That(result, Does.Contain("height=\"100%\""));
    }

    /// <summary>
    /// Tests that when only the width is specified in the input HTML, the BuildMarkup method returns markup
    /// where both the width and height attributes are set to 100%.
    /// </summary>
    [Test]
    public void BuildMarkup_WithOnlyWidthSpecified_DefaultsTo100Percent()
    {
        // Arrange
        var html = """<iframe src="https://embed.lottiefiles.com/animation/12345" width="300" height="200" frameborder="0"></iframe>""";

        // Act
        var result = LottieFiles.BuildMarkup(html, 800, null);

        // Assert
        Assert.That(result, Does.Contain("width=\"100%\""));
        Assert.That(result, Does.Contain("height=\"100%\""));
    }

    /// <summary>
    /// Tests that when only the height is specified, the width defaults to 100% in the generated markup.
    /// </summary>
    [Test]
    public void BuildMarkup_WithOnlyHeightSpecified_DefaultsTo100Percent()
    {
        // Arrange
        var html = """<iframe src="https://embed.lottiefiles.com/animation/12345" width="300" height="200" frameborder="0"></iframe>""";

        // Act
        var result = LottieFiles.BuildMarkup(html, null, 600);

        // Assert
        Assert.That(result, Does.Contain("width=\"100%\""));
        Assert.That(result, Does.Contain("height=\"100%\""));
    }
}
