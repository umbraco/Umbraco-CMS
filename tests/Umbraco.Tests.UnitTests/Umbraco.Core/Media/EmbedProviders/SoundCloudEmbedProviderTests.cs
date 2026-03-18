using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Contains unit tests for the <see cref="SoundCloudEmbedProvider"/> class, verifying its behavior and integration.
/// </summary>
[TestFixture]
public class SoundCloudEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Soundcloud(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Tests that the URL scheme regex matches valid SoundCloud URLs.
    /// </summary>
    /// <param name="url">The SoundCloud URL to test.</param>
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

    /// <summary>
    /// Tests that the URL scheme regex does not match malicious URLs to prevent SSRF attacks.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("http://127.0.0.1/soundcloud.com/artist/track")]
    [TestCase("http://localhost/soundcloud.com/artist/track")]
    [TestCase("http://example.com/soundcloud.com/artist/track")]
    [TestCase("http://example.com/redirect?url=https://soundcloud.com/artist/track")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match URLs unrelated to SoundCloud.
    /// </summary>
    /// <param name="url">The URL to test against the SoundCloud URL scheme regex.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notsoundcloud.com/artist/track")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
