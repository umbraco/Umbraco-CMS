using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Contains unit tests for the <see cref="GettyImagesEmbedProvider"/>, verifying its embedding functionality and behavior.
/// </summary>
[TestFixture]
public class GettyImagesEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new GettyImages(Mock.Of<IJsonSerializer>());

    /// <summary>
    /// Tests that the URL scheme regex correctly matches valid Getty Images URLs.
    /// </summary>
    /// <param name="url">The Getty Images URL to test.</param>
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

    /// <summary>
    /// Tests that the URL scheme regex does not match malicious URLs to prevent SSRF attacks.
    /// </summary>
    /// <param name="url">The URL to test against the regex.</param>
    [TestCase("http://127.0.0.1/gettyimages.com/detail/74917285")]
    [TestCase("http://localhost/gty.im/74917285")]
    [TestCase("http://example.com/gettyimages.com/detail/74917285")]
    [TestCase("http://example.com/redirect?url=https://gettyimages.com/detail/74917285")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    /// <summary>
    /// Verifies that the URL scheme regular expression does not incorrectly match URLs that are unrelated to Getty Images.
    /// </summary>
    /// <param name="url">A URL that should not match the Getty Images embed provider's URL scheme.</param>
    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notgettyimages.com/detail/74917285")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
