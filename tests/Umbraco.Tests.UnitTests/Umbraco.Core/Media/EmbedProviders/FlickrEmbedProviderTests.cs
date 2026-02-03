using System.Xml;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class FlickrEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Flickr(Mock.Of<IJsonSerializer>());

    private Flickr FlickrProvider => (Flickr)Provider;

    /// <summary>
    /// Tests that valid Flickr URLs are matched by the provider's URL scheme regex.
    /// </summary>
    [TestCase("https://www.flickr.com/photos/example/12345678901")]
    [TestCase("https://flickr.com/photos/example/12345678901")]
    [TestCase("http://www.flickr.com/photos/example/12345678901")]
    [TestCase("http://flickr.com/photos/example/12345678901")]
    [TestCase("https://www.flickr.com/photos/example/12345678901/in/photolist-abc123")]
    [TestCase("https://flickr.com/photos/someuser/albums/72157634123456789")]
    public void UrlSchemeRegex_MatchesValidFlickrUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    /// <summary>
    /// Tests that URLs with Flickr domain in the path (potential SSRF vector) are NOT matched.
    /// </summary>
    [TestCase("http://127.0.0.1/flickr.com/photos/example/123")]
    [TestCase("http://localhost/flickr.com/photos/example/123")]
    [TestCase("http://example.com/flickr.com/photos/example/123")]
    [TestCase("http://example.com/redirect?url=https://flickr.com/photos/example/123")]
    [TestCase("http://example.com//www.flickr.com/photos/example/123")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that unrelated URLs are not matched.
    /// </summary>
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
