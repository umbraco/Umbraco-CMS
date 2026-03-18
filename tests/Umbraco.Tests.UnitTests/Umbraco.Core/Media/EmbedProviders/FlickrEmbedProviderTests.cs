using System.Xml;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Unit tests for the <see cref="FlickrEmbedProvider"/> class.
/// </summary>
[TestFixture]
public class FlickrEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Flickr(Mock.Of<IJsonSerializer>());

    private Flickr FlickrProvider => (Flickr)Provider;

/// <summary>
/// Tests that valid Flickr URLs are matched by the provider's URL scheme regex.
/// </summary>
/// <param name="url">The Flickr URL to test against the regex.</param>
    [TestCase("https://www.flickr.com/photos/example/12345678901")]
    [TestCase("https://flickr.com/photos/example/12345678901")]
    [TestCase("http://www.flickr.com/photos/example/12345678901")]
    [TestCase("http://flickr.com/photos/example/12345678901")]
    [TestCase("https://www.flickr.com/photos/example/12345678901/in/photolist-abc123")]
    [TestCase("https://flickr.com/photos/someuser/albums/72157634123456789")]
    [TestCase("https://flic.kr/p/abc123")]
    [TestCase("http://flic.kr/p/xyz789")]
    public void UrlSchemeRegex_MatchesValidFlickrUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

/// <summary>
/// Verifies that the URL scheme regex does not match URLs where the Flickr domain appears only in the path, which could be a potential SSRF vector.
/// </summary>
/// <param name="url">A URL to test against the Flickr embed provider's URL scheme regex.</param>
    [TestCase("http://127.0.0.1/flickr.com/photos/example/123")]
    [TestCase("http://localhost/flickr.com/photos/example/123")]
    [TestCase("http://example.com/flickr.com/photos/example/123")]
    [TestCase("http://example.com/redirect?url=https://flickr.com/photos/example/123")]
    [TestCase("http://example.com//www.flickr.com/photos/example/123")]
    [TestCase("http://127.0.0.1/flic.kr/p/abc123")]
    [TestCase("http://example.com/flic.kr/p/abc123")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

/// <summary>
/// Tests that unrelated URLs are not matched.
/// </summary>
/// <param name="url">The URL to test against the regex.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://vimeo.com/123456789")]
    [TestCase("https://example.com/photos/123")]
    [TestCase("https://notflickr.com/photos/example/123")]
    [TestCase("https://flickr.org/photos/example/123")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }

    /// <summary>
    /// Verifies that <c>BuildMarkup</c> generates the correct <c>&lt;img&gt;</c> HTML tag when provided with valid Flickr oEmbed XML containing URL, width, height, and title.
    /// </summary>
    [Test]
    public void BuildImgMarkup_WithValidXml_ReturnsCorrectImgTag()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <oembed>
                <url>https://live.staticflickr.com/1234/photo_b.jpg</url>
                <width>1024</width>
                <height>768</height>
                <title>Test Photo Title</title>
            </oembed>
            """;

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        // Act
        var result = FlickrProvider.BuildMarkup(xmlDocument);

        // Assert
        Assert.That(result, Is.EqualTo("<img src=\"https://live.staticflickr.com/1234/photo_b.jpg\" width=\"1024\" height=\"768\" alt=\"Test Photo Title\" />"));
    }

    /// <summary>
    /// Tests that the BuildMarkup method correctly HTML encodes special characters in the title.
    /// </summary>
    [Test]
    public void BuildMarkup_WithSpecialCharactersInTitle_HtmlEncodesTitle()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <oembed>
                <url>https://live.staticflickr.com/1234/photo_b.jpg</url>
                <width>800</width>
                <height>600</height>
                <title>Photo with "quotes" &amp; &lt;special&gt; chars</title>
            </oembed>
            """;

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        // Act
        var result = FlickrProvider.BuildMarkup(xmlDocument);

        // Assert
        Assert.That(result, Does.Contain("alt=\"Photo with &quot;quotes&quot; &amp; &lt;special&gt; chars\""));
    }

    /// <summary>
    /// Tests that BuildMarkup returns an img tag with empty width, height, and alt attributes when elements are missing.
    /// </summary>
    [Test]
    public void BuildMarkup_WithMissingElements_ReturnsImgTagWithEmptyValues()
    {
        // Arrange
        var xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <oembed>
                <url>https://live.staticflickr.com/1234/photo_b.jpg</url>
            </oembed>
            """;

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        // Act
        var result = FlickrProvider.BuildMarkup(xmlDocument);

        // Assert
        Assert.That(result, Is.EqualTo("<img src=\"https://live.staticflickr.com/1234/photo_b.jpg\" width=\"\" height=\"\" alt=\"\" />"));
    }
}
