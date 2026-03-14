using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Contains unit tests for the <see cref="KickstarterEmbedProvider"/> class in the Umbraco.Core.Media.EmbedProviders namespace.
/// </summary>
[TestFixture]
public class KickstarterEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Kickstarter(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Tests that the URL scheme regex correctly matches valid Kickstarter URLs.
    /// </summary>
    /// <param name="url">The Kickstarter URL to test.</param>
    [TestCase("https://www.kickstarter.com/projects/creator/project-name")]
    [TestCase("https://kickstarter.com/projects/creator/project-name")]
    [TestCase("http://www.kickstarter.com/projects/creator/project-name")]
    [TestCase("http://kickstarter.com/projects/creator/project-name")]
    public void UrlSchemeRegex_MatchesValidKickstarterUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match malicious URLs to prevent SSRF attacks.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("http://127.0.0.1/kickstarter.com/projects/creator/project")]
    [TestCase("http://localhost/kickstarter.com/projects/creator/project")]
    [TestCase("http://example.com/kickstarter.com/projects/creator/project")]
    [TestCase("http://example.com/redirect?url=https://kickstarter.com/projects/creator/project")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Tests that the URL scheme regex does not match URLs unrelated to Kickstarter.
    /// </summary>
    /// <param name="url">The URL to test against the Kickstarter URL scheme regex.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notkickstarter.com/projects/creator/project")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
