using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class LottieFilesEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new LottieFiles(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.lottiefiles.com/animations/abc123")]
    [TestCase("https://lottiefiles.com/animations/abc123")]
    [TestCase("http://www.lottiefiles.com/animations/abc123")]
    [TestCase("http://lottiefiles.com/animations/abc123")]
    public void UrlSchemeRegex_MatchesValidLottieFilesUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/lottiefiles.com/animations/abc123")]
    [TestCase("http://localhost/lottiefiles.com/animations/abc123")]
    [TestCase("http://example.com/lottiefiles.com/animations/abc123")]
    [TestCase("http://example.com/redirect?url=https://lottiefiles.com/animations/abc123")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notlottiefiles.com/animations/abc123")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
