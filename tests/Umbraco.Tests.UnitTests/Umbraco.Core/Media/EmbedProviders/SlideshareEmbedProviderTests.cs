using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Contains unit tests for the <see cref="SlideshareEmbedProvider"/> class, verifying its embedding functionality and behavior.
/// </summary>
[TestFixture]
public class SlideshareEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Slideshare(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Tests that the URL scheme regex correctly matches valid SlideShare URLs.
    /// </summary>
    /// <param name="url">The SlideShare URL to test against the regex.</param>
    [TestCase("https://www.slideshare.net/username/presentation")]
    [TestCase("https://slideshare.net/username/presentation")]
    [TestCase("http://www.slideshare.net/username/presentation")]
    [TestCase("http://slideshare.net/username/presentation")]
    public void UrlSchemeRegex_MatchesValidSlideshareUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match malicious URLs to prevent SSRF attacks.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("http://127.0.0.1/slideshare.net/username/presentation")]
    [TestCase("http://localhost/slideshare.net/username/presentation")]
    [TestCase("http://example.com/slideshare.net/username/presentation")]
    [TestCase("http://example.com/redirect?url=https://slideshare.net/username/presentation")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match URLs unrelated to SlideShare.
    /// </summary>
    /// <param name="url">The URL to test against the SlideShare URL scheme regex.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notslideshare.net/username/presentation")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
