using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class VimeoEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Vimeo(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.vimeo.com/123456789")]
    [TestCase("https://vimeo.com/123456789")]
    [TestCase("http://www.vimeo.com/123456789")]
    [TestCase("http://vimeo.com/123456789")]
    [TestCase("https://vimeo.com/channels/staffpicks/123456789")]
    public void UrlSchemeRegex_MatchesValidVimeoUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/vimeo.com/123456789")]
    [TestCase("http://localhost/vimeo.com/123456789")]
    [TestCase("http://example.com/vimeo.com/123456789")]
    [TestCase("http://example.com/redirect?url=https://vimeo.com/123456789")]
    [TestCase("http://example.com//www.vimeo.com/123456789")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notvimeo.com/123456789")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
