using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentUrlFactory : IDocumentUrlFactory
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly ILanguageService _languageService;
    private readonly IPublishedRouter _publishedRouter;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly ILogger<DocumentUrlFactory> _logger;
    private readonly UriUtility _uriUtility;


    [Obsolete("Use the constructor that takes all dependencies, scheduled for removal in v16")]
    public DocumentUrlFactory(IDocumentUrlService documentUrlService)
    : this(
        StaticServiceProvider.Instance.GetRequiredService<IPublishedUrlProvider>(),
        StaticServiceProvider.Instance.GetRequiredService<ILanguageService>(),
        StaticServiceProvider.Instance.GetRequiredService<IPublishedRouter>(),
        StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>(),
        StaticServiceProvider.Instance.GetRequiredService<ILocalizedTextService>(),
        StaticServiceProvider.Instance.GetRequiredService<ILogger<DocumentUrlFactory>>(),
        StaticServiceProvider.Instance.GetRequiredService<UriUtility>()
        )
    {
    }

    [Obsolete("Use the constructor that takes all dependencies, scheduled for removal in v16")]
    public DocumentUrlFactory(
        IDocumentUrlService documentUrlService,
        IPublishedUrlProvider publishedUrlProvider,
        ILanguageService languageService,
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor,
        ILocalizedTextService localizedTextService,
        ILogger<DocumentUrlFactory> logger,
        UriUtility uriUtility)
    : this(publishedUrlProvider,
        languageService,
        publishedRouter,
        umbracoContextAccessor,
        localizedTextService,
        logger,
        uriUtility)
    {
        _publishedUrlProvider = publishedUrlProvider;
        _languageService = languageService;
        _publishedRouter = publishedRouter;
        _umbracoContextAccessor = umbracoContextAccessor;
        _localizedTextService = localizedTextService;
        _logger = logger;
        _uriUtility = uriUtility;
    }

    public DocumentUrlFactory(
        IPublishedUrlProvider publishedUrlProvider,
        ILanguageService languageService,
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor,
        ILocalizedTextService localizedTextService,
        ILogger<DocumentUrlFactory> logger,
        UriUtility uriUtility)
    {
        _publishedUrlProvider = publishedUrlProvider;
        _languageService = languageService;
        _publishedRouter = publishedRouter;
        _umbracoContextAccessor = umbracoContextAccessor;
        _localizedTextService = localizedTextService;
        _logger = logger;
        _uriUtility = uriUtility;
    }

    public async Task<IEnumerable<DocumentUrlInfo>> CreateUrlsAsync(IContent content)
    {
        HashSet<UrlInfo> urlInfos = new();
        var cultures = (await _languageService.GetAllAsync()).Select(x => x.IsoCode).ToArray();

        // First we get the urls of all cultures, using the published router, meaning we respect any extensions.
        foreach (var culture in cultures)
        {
            var url = _publishedUrlProvider.GetUrl(content.Key, culture: culture);
            // Check for collision
            Attempt<UrlInfo?> hasCollision = await VerifyCollisionAsync(content, url, culture);

            if (hasCollision.Success && hasCollision.Result is not null)
            {
                urlInfos.Add(hasCollision.Result);
                continue;
            }

            urlInfos.Add(UrlInfo.Url(url, culture));
        }

        // Then get "other" urls - I.E. Not what you'd get with GetUrl(), this includes all the urls registered using domains.
        // for these 'other' URLs, we don't check whether they are routable, collide, anything - we just report them.
        foreach (UrlInfo otherUrl in _publishedUrlProvider.GetOtherUrls(content.Id).OrderBy(x => x.Text).ThenBy(x => x.Culture))
        {
            urlInfos.Add(otherUrl);
        }

        return urlInfos
            .Where(urlInfo => urlInfo.IsUrl)
            .Select(urlInfo => new DocumentUrlInfo { Culture = urlInfo.Culture, Url = urlInfo.Text })
            .ToArray();
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
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                const string logMsg = nameof(VerifyCollisionAsync) +
                                      " did not resolve a content item for original url: {Url}, translated to {TranslatedUrl} and culture: {Culture}";
                _logger.LogDebug(logMsg, url, uri, culture);
            }

            var urlInfo = UrlInfo.Message(_localizedTextService.Localize("content", "routeErrorCannotRoute"), culture);
            return Attempt.Succeed(urlInfo);
        }

        if (publishedRequest.IgnorePublishedContentCollisions)
        {
            return Attempt<UrlInfo?>.Fail();
        }

        if (publishedRequest.PublishedContent?.Id != content.Id)
        {
            var urlInfo = UrlInfo.Message(_localizedTextService.Localize("content", "routeError"), culture);
            return Attempt.Succeed(urlInfo);
        }

        // No collision
        return Attempt<UrlInfo?>.Fail();
    }

    public async Task<IEnumerable<DocumentUrlInfoResponseModel>> CreateUrlSetsAsync(IEnumerable<IContent> contentItems)
    {
        var documentUrlInfoResourceSets = new List<DocumentUrlInfoResponseModel>();

        foreach (IContent content in contentItems)
        {
            IEnumerable<DocumentUrlInfo> urls = await CreateUrlsAsync(content);
            documentUrlInfoResourceSets.Add(new DocumentUrlInfoResponseModel(content.Key, urls));
        }

        return documentUrlInfoResourceSets;
    }
}
