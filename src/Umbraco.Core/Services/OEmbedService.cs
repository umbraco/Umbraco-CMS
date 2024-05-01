using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class OEmbedService : IOEmbedService
{
    private readonly EmbedProvidersCollection _embedProvidersCollection;
    private readonly ILogger<OEmbedService> _logger;

    public OEmbedService(EmbedProvidersCollection embedProvidersCollection, ILogger<OEmbedService> logger)
    {
        _embedProvidersCollection = embedProvidersCollection;
        _logger = logger;
    }

    public async Task<Attempt<string, OEmbedOperationStatus>> GetMarkupAsync(Uri url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        // Find the first provider that supports the URL
        IEmbedProvider? matchedProvider = _embedProvidersCollection
            .FirstOrDefault(provider => provider.UrlSchemeRegex.Any(regex=>new Regex(regex, RegexOptions.IgnoreCase).IsMatch(url.OriginalString)));

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
            _logger.LogError(e, "Unexpected exception happened while trying to get oembed markup. Provider: {Provider}",matchedProvider.GetType().Name);
            Attempt.FailWithStatus(OEmbedOperationStatus.UnexpectedException, string.Empty, e);
        }

        return Attempt.FailWithStatus(OEmbedOperationStatus.ProviderReturnedInvalidResult, string.Empty);
    }
}
