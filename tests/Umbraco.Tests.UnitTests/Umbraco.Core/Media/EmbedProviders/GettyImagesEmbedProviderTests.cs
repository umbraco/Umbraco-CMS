using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class GettyImagesEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new GettyImages(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.gettyimages.com/detail/74917285")]
    [TestCase("https://gettyimages.com/detail/74917285")]
    [TestCase("http://www.gettyimages.com/detail/74917285")]
    [TestCase("http://gettyimages.com/detail/74917285")]
    [TestCase("https://www.gty.im/74917285")]
    [TestCase("https://gty.im/74917285")]
    [TestCase("http://gty.im/74917285")]
    public void UrlSchemeRegex_MatchesValidGettyImagesUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/gettyimages.com/detail/74917285")]
    [TestCase("http://localhost/gty.im/74917285")]
    [TestCase("http://example.com/gettyimages.com/detail/74917285")]
    [TestCase("http://example.com/redirect?url=https://gettyimages.com/detail/74917285")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notgettyimages.com/detail/74917285")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
