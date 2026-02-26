using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

[TestFixture]
public class KickstarterEmbedProviderTests : OEmbedProviderTestBase
{
    protected override IEmbedProvider Provider { get; } = new Kickstarter(Mock.Of<IJsonSerializer>());

    [TestCase("https://www.kickstarter.com/projects/creator/project-name")]
    [TestCase("https://kickstarter.com/projects/creator/project-name")]
    [TestCase("http://www.kickstarter.com/projects/creator/project-name")]
    [TestCase("http://kickstarter.com/projects/creator/project-name")]
    public void UrlSchemeRegex_MatchesValidKickstarterUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.True, $"Expected URL to match: {url}");
    }

    [TestCase("http://127.0.0.1/kickstarter.com/projects/creator/project")]
    [TestCase("http://localhost/kickstarter.com/projects/creator/project")]
    [TestCase("http://example.com/kickstarter.com/projects/creator/project")]
    [TestCase("http://example.com/redirect?url=https://kickstarter.com/projects/creator/project")]
    public void UrlSchemeRegex_DoesNotMatchMaliciousUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match (SSRF protection): {url}");
    }

    [TestCase("https://www.youtube.com/watch?v=abc123")]
    [TestCase("https://notkickstarter.com/projects/creator/project")]
    public void UrlSchemeRegex_DoesNotMatchUnrelatedUrls(string url)
    {
        var result = MatchesUrlScheme(url);

        Assert.That(result, Is.False, $"Expected URL to NOT match: {url}");
    }
}
