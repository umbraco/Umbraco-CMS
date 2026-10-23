using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class WistiaEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Wistia(Mock.Of<IJsonSerializer>());

    [TestCase("https://home.wistia.com/medias/e4a27b971d")]
    [TestCase("https://acme.wistia.com/medias/e4a27b971d/manage")]
    [TestCase("https://wistia.com/medias/e4a27b971d")]
    [TestCase("http://home.wistia.com/medias/e4a27b971d")]
    [TestCase("https://acme.wistia.com/m/abc123")]
    [TestCase("https://acme.wistia.com/s/abc123")]
    [TestCase("https://fast.wistia.com/embed/channel/9c5u5vvz1d")]
    [TestCase("https://wi.st/medias/abc123")]
    [TestCase("https://www.wi.st/m/abc123")]
    public void UrlSchemeRegex_MatchesValidWistiaUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/wistia.com/medias/abc123")]
    [TestCase("http://localhost/home.wistia.com/medias/abc123")]
    [TestCase("http://example.com/home.wistia.com/medias/abc123")]
    [TestCase("http://example.com/redirect?url=https://home.wistia.com/medias/abc123")]
    [TestCase("http://example.com//home.wistia.com/medias/abc123")]
    [TestCase("https://wistia.com.example.com/medias/abc123")]
    [TestCase("https://example.com@home.wistia.com/medias/abc123")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.vimeo.com/123456789")]
    [TestCase("https://notwistia.com/medias/abc123")]
    [TestCase("https://acme.wistia.com/channels/abc123")]
    [TestCase("https://acme.wistia.com/live/events/abc123")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedOrUnsupportedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
