using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Contains unit tests for the <see cref="XEmbedProvider"/> class to verify its behavior and functionality.
/// </summary>
[TestFixture]
public class XEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new X(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Tests that the URL scheme regex correctly matches valid X URLs.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("https://x.com/user/status/1234567890")]
    [TestCase("https://www.x.com/user/status/1234567890")]
    [TestCase("http://x.com/user/status/1234567890")]
    [TestCase("https://twitter.com/user/status/1234567890")]
    [TestCase("https://www.twitter.com/user/status/1234567890")]
    [TestCase("http://twitter.com/user/status/1234567890")]
    [TestCase("https://x.com/THR/status/1995620384344080849?s=20")]
    public void UrlSchemeRegex_MatchesValidXUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match malicious URLs to prevent SSRF attacks.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("http://127.0.0.1/x.com/user/status/1234567890")]
    [TestCase("http://localhost/twitter.com/user/status/1234567890")]
    [TestCase("http://example.com/x.com/user/status/1234567890")]
    [TestCase("http://example.com/redirect?url=https://x.com/user/status/1234567890")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match URLs that are unrelated to the intended pattern.
    /// Ensures that only valid URLs are matched by the regex and unrelated URLs are correctly excluded.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notx.com/user/status/1234567890")]
    [TestCase("https://x.com/user/profile")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }

/// <summary>
/// Tests that URLs with extra path segments before /status/ are not matched.
/// The username should be a single path segment.
/// </summary>
/// <param name="url">The URL to test for invalid extra path segments before <c>/status/</c>.</param>
    [TestCase("https://x.com/user/extra/status/1234567890")]
    [TestCase("https://twitter.com/user/extra/path/status/1234567890")]
    [TestCase("https://x.com/user/nested/deep/status/1234567890")]
    public void UrlSchemeRegex_DoesNotMatchUrlsWithExtraPathSegments(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (extra path segments): {url}");
    }
}
