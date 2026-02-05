using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class IssuuEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Issuu(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.issuu.com/username/docs/document")]
    [TestCase("https://issuu.com/username/docs/document")]
    [TestCase("http://www.issuu.com/username/docs/document")]
    [TestCase("http://issuu.com/username/docs/document")]
    [TestCase("https://issuu.com/someuser/docs/my-document-name")]
    public void UrlSchemeRegex_MatchesValidIssuuUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/issuu.com/username/docs/document")]
    [TestCase("http://localhost/issuu.com/username/docs/document")]
    [TestCase("http://example.com/issuu.com/username/docs/document")]
    [TestCase("http://example.com/redirect?url=https://issuu.com/username/docs/document")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notissuu.com/username/docs/document")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }

    /// <summary>
    /// Tests that URLs with extra path segments before /docs/ are not matched.
    /// The publisher name should be a single path segment.
    /// </summary>
    [TestCase("https://issuu.com/publisher/extra/docs/document")]
    [TestCase("https://issuu.com/publisher/nested/path/docs/document")]
    public void UrlSchemeRegex_DoesNotMatchUrlsWithExtraPathSegments(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (extra path segments): {url}");
    }
}
