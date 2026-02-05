using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class GiphyEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Giphy(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.giphy.com/gifs/abc123")]
    [TestCase("https://giphy.com/gifs/abc123")]
    [TestCase("http://www.giphy.com/gifs/abc123")]
    [TestCase("http://giphy.com/gifs/abc123")]
    [TestCase("https://www.gph.is/abc123")]
    [TestCase("https://gph.is/abc123")]
    [TestCase("http://gph.is/abc123")]
    public void UrlSchemeRegex_MatchesValidGiphyUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/giphy.com/gifs/abc123")]
    [TestCase("http://localhost/gph.is/abc123")]
    [TestCase("http://example.com/giphy.com/gifs/abc123")]
    [TestCase("http://example.com/redirect?url=https://giphy.com/gifs/abc123")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notgiphy.com/gifs/abc123")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
