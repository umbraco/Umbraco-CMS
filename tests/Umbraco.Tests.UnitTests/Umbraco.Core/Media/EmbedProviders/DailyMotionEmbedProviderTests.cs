using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class DailyMotionEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new DailyMotion(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.dailymotion.com/video/x123abc")]
    [TestCase("https://dailymotion.com/video/x123abc")]
    [TestCase("http://www.dailymotion.com/video/x123abc")]
    [TestCase("http://dailymotion.com/video/x123abc")]
    public void UrlSchemeRegex_MatchesValidDailyMotionUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/dailymotion.com/video/x123abc")]
    [TestCase("http://localhost/dailymotion.com/video/x123abc")]
    [TestCase("http://example.com/dailymotion.com/video/x123abc")]
    [TestCase("http://example.com/redirect?url=https://dailymotion.com/video/x123abc")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notdailymotion.com/video/x123abc")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
