using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class SlideshareEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Slideshare(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.slideshare.net/username/presentation")]
    [TestCase("https://slideshare.net/username/presentation")]
    [TestCase("http://www.slideshare.net/username/presentation")]
    [TestCase("http://slideshare.net/username/presentation")]
    public void UrlSchemeRegex_MatchesValidSlideshareUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/slideshare.net/username/presentation")]
    [TestCase("http://localhost/slideshare.net/username/presentation")]
    [TestCase("http://example.com/slideshare.net/username/presentation")]
    [TestCase("http://example.com/redirect?url=https://slideshare.net/username/presentation")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notslideshare.net/username/presentation")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
