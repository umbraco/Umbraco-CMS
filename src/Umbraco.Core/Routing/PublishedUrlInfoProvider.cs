using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides the default implementation of <see cref="IPublishedUrlInfoProvider" />.
/// </summary>
public class PublishedUrlInfoProvider : IPublishedUrlInfoProvider
{
    private const string UrlProviderAlias = Constants.UrlProviders.Content;

    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly ILanguageService _languageService;
    private readonly IPublishedRouter _publishedRouter;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly ILogger<PublishedUrlInfoProvider> _logger;
    private readonly UriUtility _uriUtility;
    private readonly IVariationContextAccessor _variationContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedUrlInfoProvider" /> class.
    /// </summary>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="languageService">The language service.</param>
    /// <param name="publishedRouter">The published router.</param>
    /// <param name="umbracoContextAccessor">The Umbraco context accessor.</param>
    /// <param name="localizedTextService">The localized text service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="uriUtility">The URI utility.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    public PublishedUrlInfoProvider(
        IPublishedUrlProvider publishedUrlProvider,
        ILanguageService languageService,
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor,
        ILocalizedTextService localizedTextService,
        ILogger<PublishedUrlInfoProvider> logger,
        UriUtility uriUtility,
        IVariationContextAccessor variationContextAccessor)
    {
        _publishedUrlProvider = publishedUrlProvider;
        _languageService = languageService;
        _publishedRouter = publishedRouter;
        _umbracoContextAccessor = umbracoContextAccessor;
        _localizedTextService = localizedTextService;
        _logger = logger;
        _uriUtility = uriUtility;
        _variationContextAccessor = variationContextAccessor;
    }

    /// <inheritdoc />
    public async Task<ISet<UrlInfo>> GetAllAsync(IContent content)
    {
        HashSet<UrlInfo> urlInfos = [];
        IEnumerable<string> cultures = await _languageService.GetAllIsoCodesAsync();

        // First we get the urls of all cultures, using the published router, meaning we respect any extensions.
        foreach (var culture in cultures)
        {
            var url = _publishedUrlProvider.GetUrl(content.Key, culture: culture);

            // Handle "could not get URL"
            if (url is "#" or "#ex")
            {
                urlInfos.Add(UrlInfo.AsMessage(_localizedTextService.Localize("content", "getUrlException"), UrlProviderAlias, culture));
                continue;
            }

            // Check for collision
            Attempt<UrlInfo?> hasCollision = await VerifyCollisionAsync(content, url, culture);

            if (hasCollision is { Success: true, Result: not null })
            {
                urlInfos.Add(hasCollision.Result);
                continue;
            }

            urlInfos.Add(UrlInfo.AsUrl(url, UrlProviderAlias, culture));
        }

        // If the content is trashed, we can't get the other URLs, as we have no parent structure to navigate through.
        if (content.Trashed)
        {
            return urlInfos;
        }

        // Then get "other" urls - I.E. Not what you'd get with GetUrl(), this includes all the urls registered using domains.
        // for these 'other' URLs, we don't check whether they are routable, collide, anything - we just report them.
        foreach (UrlInfo otherUrl in _publishedUrlProvider.GetOtherUrls(content.Id).OrderBy(x => x.Message).ThenBy(x => x.Culture))
        {
            urlInfos.Add(otherUrl);
        }

        return urlInfos;
    }

    private async Task<Attempt<UrlInfo?>> VerifyCollisionAsync(IContent content, string url, string culture)
    {
        var uri = new Uri(url.TrimEnd(Constants.CharArrays.ForwardSlash), UriKind.RelativeOrAbsolute);
        if (uri.IsAbsoluteUri is false)
        {
            uri = uri.MakeAbsolute(_umbracoContextAccessor.GetRequiredUmbracoContext().CleanedUmbracoUrl);
        }

        uri = _uriUtility.UriToUmbraco(uri);
        IPublishedRequestBuilder builder = await _publishedRouter.CreateRequestAsync(uri);
        IPublishedRequest publishedRequest = await _publishedRouter.RouteRequestAsync(builder, new RouteRequestOptions(RouteDirection.Outbound));

        if (publishedRequest.HasPublishedContent() is false)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                const string logMsg = nameof(VerifyCollisionAsync) +
                                      " did not resolve a content item for original url: {Url}, translated to {TranslatedUrl} and culture: {Culture}";
                _logger.LogDebug(logMsg, url, uri, culture);
            }

            var urlInfo = UrlInfo.AsMessage(_localizedTextService.Localize("content", "routeErrorCannotRoute"), UrlProviderAlias, culture);
            return Attempt.Succeed(urlInfo);
        }

        if (publishedRequest.IgnorePublishedContentCollisions)
        {
            return Attempt<UrlInfo?>.Fail();
        }

        if (publishedRequest.PublishedContent?.Id != content.Id)
        {
            var collidingContent = publishedRequest.PublishedContent?.Key.ToString();

            var urlInfo = UrlInfo.AsMessage(_localizedTextService.Localize("content", "routeError", [collidingContent]), UrlProviderAlias, culture);
            return Attempt.Succeed(urlInfo);
        }

        // No collision
        return Attempt<UrlInfo?>.Fail();
    }
}
