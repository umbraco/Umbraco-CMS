using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class TedEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Ted(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.ted.com/talks/speaker_talk_title")]
    [TestCase("https://ted.com/talks/speaker_talk_title")]
    [TestCase("http://www.ted.com/talks/speaker_talk_title")]
    [TestCase("http://ted.com/talks/speaker_talk_title")]
    public void UrlSchemeRegex_MatchesValidTedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/ted.com/talks/speaker_talk_title")]
    [TestCase("http://localhost/ted.com/talks/speaker_talk_title")]
    [TestCase("http://example.com/ted.com/talks/speaker_talk_title")]
    [TestCase("http://example.com/redirect?url=https://ted.com/talks/speaker_talk_title")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notted.com/talks/speaker_talk_title")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
