using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Implements <see cref="IOEmbedService"/> for retrieving embeddable HTML markup using the oEmbed protocol.
/// </summary>
public class OEmbedService : IOEmbedService
{
    private static readonly ConcurrentDictionary<string, Regex> RegexCache = new();

    private readonly EmbedProvidersCollection _embedProvidersCollection;
    private readonly ILogger<OEmbedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OEmbedService"/> class.
    /// </summary>
    public OEmbedService(EmbedProvidersCollection embedProvidersCollection, ILogger<OEmbedService> logger)
    {
        _embedProvidersCollection = embedProvidersCollection;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Attempt<string, OEmbedOperationStatus>> GetMarkupAsync(Uri url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        // Find the first provider that supports the URL
        IEmbedProvider? matchedProvider = _embedProvidersCollection
            .FirstOrDefault(provider => MatchesUrlScheme(url.OriginalString, provider.UrlSchemeRegex));

        if (matchedProvider is null)
        {
            return Attempt.FailWithStatus(OEmbedOperationStatus.NoSupportedProvider, string.Empty);
        }

        try
        {
            var result = await matchedProvider.GetMarkupAsync(url.OriginalString, maxWidth, maxHeight, cancellationToken);

            if (result is not null)
            {
                return Attempt.SucceedWithStatus(OEmbedOperationStatus.Success, result);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected exception happened while trying to get oEmbed markup. Provider: {Provider}", matchedProvider.GetType().Name);
            return Attempt.FailWithStatus(OEmbedOperationStatus.UnexpectedException, string.Empty, e);
        }

        return Attempt.FailWithStatus(OEmbedOperationStatus.ProviderReturnedInvalidResult, string.Empty);
    }

    /// <summary>
    /// Determines whether a URL matches any of the provider's URL scheme regex patterns.
    /// </summary>
    /// <param name="url">The URL to test.</param>
    /// <param name="urlSchemeRegexPatterns">The regex patterns to match against.</param>
    /// <returns><c>true</c> if the URL matches any pattern; otherwise, <c>false</c>.</returns>
    internal static bool MatchesUrlScheme(string url, string[] urlSchemeRegexPatterns)
        => urlSchemeRegexPatterns.Any(pattern => GetOrCreateRegex(pattern).IsMatch(url));

    private static Regex GetOrCreateRegex(string pattern)
        => RegexCache.GetOrAdd(pattern, p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled));
}
