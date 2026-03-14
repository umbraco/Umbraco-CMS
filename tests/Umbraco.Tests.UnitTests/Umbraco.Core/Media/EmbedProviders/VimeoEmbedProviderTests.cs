using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Contains unit tests for the <see cref="VimeoEmbedProvider"/> class, verifying Vimeo embed functionality in Umbraco CMS.
/// </summary>
[TestFixture]
public class VimeoEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Vimeo(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Tests that the URL scheme regex correctly matches valid Vimeo URLs.
    /// </summary>
    /// <param name="url">The Vimeo URL to test against the regex.</param>
    [TestCase("https://www.vimeo.com/123456789")]
    [TestCase("https://vimeo.com/123456789")]
    [TestCase("http://www.vimeo.com/123456789")]
    [TestCase("http://vimeo.com/123456789")]
    [TestCase("https://vimeo.com/channels/staffpicks/123456789")]
    public void UrlSchemeRegex_MatchesValidVimeoUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    /// <summary>
    /// Verifies that the URL scheme regex used for Vimeo embed detection does not incorrectly match potentially malicious URLs,
    /// such as those that could be used for SSRF (Server-Side Request Forgery) attacks.
    /// </summary>
    /// <param name="url">The potentially malicious URL to test against the Vimeo URL scheme regex.</param>
    /// <remarks>
    /// This test helps ensure that only valid Vimeo URLs are matched and embedded, improving security.
    /// </remarks>
    [TestCase("http://127.0.0.1/vimeo.com/123456789")]
    [TestCase("http://localhost/vimeo.com/123456789")]
    [TestCase("http://example.com/vimeo.com/123456789")]
    [TestCase("http://example.com/redirect?url=https://vimeo.com/123456789")]
    [TestCase("http://example.com//www.vimeo.com/123456789")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match URLs unrelated to Vimeo.
    /// </summary>
    /// <param name="url">The URL to test against the Vimeo URL scheme regex.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notvimeo.com/123456789")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
