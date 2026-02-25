using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media.EmbedProviders;

/// <summary>
/// Base class for OEmbed provider URL scheme tests.
/// </summary>
public abstract class OEmbedProviderTestBase
{
    /// <summary>
    /// Gets the embed provider instance to test.
    /// </summary>
    protected abstract IEmbedProvider Provider { get; }

    /// <summary>
    /// Matches URL against provider patterns using the same logic as OEmbedService.
    /// </summary>
    protected bool MatchesUrlScheme(string url)
        => OEmbedService.MatchesUrlScheme(url, Provider.UrlSchemeRegex);
}
