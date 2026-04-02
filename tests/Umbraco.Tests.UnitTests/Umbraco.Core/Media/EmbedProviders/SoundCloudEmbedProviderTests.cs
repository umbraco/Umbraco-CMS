using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class SoundCloudEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Soundcloud(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.soundcloud.com/artist/track")]
    [TestCase("https://soundcloud.com/artist/track")]
    [TestCase("http://www.soundcloud.com/artist/track")]
    [TestCase("http://soundcloud.com/artist/track")]
    [TestCase("https://soundcloud.com/artist/sets/playlist")]
    public void UrlSchemeRegex_MatchesValidSoundCloudUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/soundcloud.com/artist/track")]
    [TestCase("http://localhost/soundcloud.com/artist/track")]
    [TestCase("http://example.com/soundcloud.com/artist/track")]
    [TestCase("http://example.com/redirect?url=https://soundcloud.com/artist/track")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notsoundcloud.com/artist/track")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
