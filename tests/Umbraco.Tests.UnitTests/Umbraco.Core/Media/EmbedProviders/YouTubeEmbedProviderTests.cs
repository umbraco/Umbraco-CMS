using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class YouTubeEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new YouTube(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://youtube.com/watch?v=abc123")]
    [TestCase("http://www.youtube.com/watch?v=abc123")]
    [TestCase("https://www.youtu.be/abc123")]
    [TestCase("https://youtu.be/abc123")]
    [TestCase("https://www.youtube.com/shorts/abc123")]
    [TestCase("https://youtube.com/shorts/abc123")]
    [TestCase("https://www.youtube.com/live/abc123")]
    [TestCase("https://youtube.com/live/abc123")]
    public void UrlSchemeRegex_MatchesValidYouTubeUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/youtube.com/watch?v=abc123")]
    [TestCase("http://localhost/youtube.com/watch?v=abc123")]
    [TestCase("http://example.com/youtube.com/watch?v=abc123")]
    [TestCase("http://example.com/redirect?url=https://youtube.com/watch?v=abc123")]
    [TestCase("http://example.com//www.youtube.com/watch?v=abc123")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.vimeo.com/123456789")]
    [TestCase("https://flickr.com/photos/example/123")]
    [TestCase("https://notyoutube.com/watch?v=abc123")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
