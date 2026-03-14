using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Contains unit tests for the <see cref="TedEmbedProvider"/> class, verifying its embedding functionality.
/// </summary>
[TestFixture]
public class TedEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Ted(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Tests that the URL scheme regex correctly matches valid TED URLs.
    /// </summary>
    /// <param name="url">The TED URL to test against the regex.</param>
    [TestCase("https://www.ted.com/talks/speaker_talk_title")]
    [TestCase("https://ted.com/talks/speaker_talk_title")]
    [TestCase("http://www.ted.com/talks/speaker_talk_title")]
    [TestCase("http://ted.com/talks/speaker_talk_title")]
    public void UrlSchemeRegex_MatchesValidTedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    /// <summary>
    /// Verifies that the URL scheme regex does not incorrectly match potentially malicious URLs, helping to prevent SSRF (Server-Side Request Forgery) attacks.
    /// </summary>
    /// <param name="url">A potentially malicious URL to test against the embed provider's URL scheme regex.</param>
    /// <remarks>
    /// The test asserts that the regex does not match URLs that could be used to exploit SSRF vulnerabilities.
    /// </remarks>
    [TestCase("http://127.0.0.1/ted.com/talks/speaker_talk_title")]
    [TestCase("http://localhost/ted.com/talks/speaker_talk_title")]
    [TestCase("http://example.com/ted.com/talks/speaker_talk_title")]
    [TestCase("http://example.com/redirect?url=https://ted.com/talks/speaker_talk_title")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match unrelated URLs.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notted.com/talks/speaker_talk_title")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
