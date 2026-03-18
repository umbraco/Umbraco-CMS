using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Contains unit tests for the <see cref="HuluEmbedProvider"/> class, verifying its embed functionality for Hulu media.
/// </summary>
[TestFixture]
public class HuluEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Hulu(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Verifies that the URL scheme regex matches valid Hulu URLs.
    /// </summary>
    /// <param name="url">A Hulu URL to test against the URL scheme regex.</param>
    [TestCase("https://www.hulu.com/watch/abc123")]
    [TestCase("https://hulu.com/watch/abc123")]
    [TestCase("http://www.hulu.com/watch/abc123")]
    [TestCase("http://hulu.com/watch/abc123")]
    public void UrlSchemeRegex_MatchesValidHuluUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match malicious URLs to prevent SSRF attacks.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("http://127.0.0.1/hulu.com/watch/abc123")]
    [TestCase("http://localhost/hulu.com/watch/abc123")]
    [TestCase("http://example.com/hulu.com/watch/abc123")]
    [TestCase("http://example.com/redirect?url=https://hulu.com/watch/abc123")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match URLs unrelated to Hulu.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://nothulu.com/watch/abc123")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
