using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Contains unit tests for the <see cref="DailyMotionEmbedProvider"/> class, verifying its embedding functionality.
/// </summary>
[TestFixture]
public class DailyMotionEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new DailyMotion(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Tests that the URL scheme regex matches valid DailyMotion URLs.
    /// </summary>
    /// <param name="url">The DailyMotion URL to test.</param>
    [TestCase("https://www.dailymotion.com/video/x123abc")]
    [TestCase("https://dailymotion.com/video/x123abc")]
    [TestCase("http://www.dailymotion.com/video/x123abc")]
    [TestCase("http://dailymotion.com/video/x123abc")]
    public void UrlSchemeRegex_MatchesValidDailyMotionUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    /// <summary>
    /// Verifies that the URL scheme regex does not match malicious URLs to prevent SSRF attacks.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("http://127.0.0.1/dailymotion.com/video/x123abc")]
    [TestCase("http://localhost/dailymotion.com/video/x123abc")]
    [TestCase("http://example.com/dailymotion.com/video/x123abc")]
    [TestCase("http://example.com/redirect?url=https://dailymotion.com/video/x123abc")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match URLs unrelated to DailyMotion.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notdailymotion.com/video/x123abc")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
