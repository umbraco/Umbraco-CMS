using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class XEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new X(Mock.Of<IJsonSerializer>());

    [TestCase("https://x.com/user/status/1234567890")]
    [TestCase("https://www.x.com/user/status/1234567890")]
    [TestCase("http://x.com/user/status/1234567890")]
    [TestCase("https://twitter.com/user/status/1234567890")]
    [TestCase("https://www.twitter.com/user/status/1234567890")]
    [TestCase("http://twitter.com/user/status/1234567890")]
    [TestCase("https://x.com/THR/status/1995620384344080849?s=20")]
    public void UrlSchemeRegex_MatchesValidXUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/x.com/user/status/1234567890")]
    [TestCase("http://localhost/twitter.com/user/status/1234567890")]
    [TestCase("http://example.com/x.com/user/status/1234567890")]
    [TestCase("http://example.com/redirect?url=https://x.com/user/status/1234567890")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notx.com/user/status/1234567890")]
    [TestCase("https://x.com/user/profile")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
