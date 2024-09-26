using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.ViewModels.Document;
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
    private readonly IPublishedRouter _publishedRouter;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ILanguageService _languageService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IContentService _contentService;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly ILoggerFactory _loggerFactory;
    private readonly UriUtility _uriUtility;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IPublishedContentCache _contentCache;
    private readonly IDocumentNavigationQueryService _navigationQueryService;

    [Obsolete("Use the constructor that takes all parameters. Scheduled for removal in V17.")]
    public DocumentUrlFactory(
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor,
        ILanguageService languageService,
        ILocalizedTextService localizedTextService,
        IContentService contentService,
        IVariationContextAccessor variationContextAccessor,
        ILoggerFactory loggerFactory,
        UriUtility uriUtility,
        IPublishedUrlProvider publishedUrlProvider)
        : this(
            publishedRouter,
            umbracoContextAccessor,
            languageService,
            localizedTextService,
            contentService,
            variationContextAccessor,
            loggerFactory,
            uriUtility,
            publishedUrlProvider,
            StaticServiceProvider.Instance.GetRequiredService<IPublishedContentCache>(),
            StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>())
    {
    }

    public DocumentUrlFactory(
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor,
        ILanguageService languageService,
        ILocalizedTextService localizedTextService,
        IContentService contentService,
        IVariationContextAccessor variationContextAccessor,
        ILoggerFactory loggerFactory,
        UriUtility uriUtility,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedContentCache contentCache,
        IDocumentNavigationQueryService navigationQueryService)
    {
        _publishedRouter = publishedRouter;
        _umbracoContextAccessor = umbracoContextAccessor;
        _languageService = languageService;
        _localizedTextService = localizedTextService;
        _contentService = contentService;
        _variationContextAccessor = variationContextAccessor;
        _loggerFactory = loggerFactory;
        _uriUtility = uriUtility;
        _publishedUrlProvider = publishedUrlProvider;
        _contentCache = contentCache;
        _navigationQueryService = navigationQueryService;
    }

    public async Task<IEnumerable<DocumentUrlInfo>> CreateUrlsAsync(IContent content)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();

        IEnumerable<UrlInfo> urlInfos = await content.GetContentUrlsAsync(
            _publishedRouter,
            umbracoContext,
            _languageService,
            _localizedTextService,
            _contentService,
            _variationContextAccessor,
            _loggerFactory.CreateLogger<IContent>(),
            _uriUtility,
            _publishedUrlProvider,
            _contentCache,
            _navigationQueryService);

        return urlInfos
            .Where(urlInfo => urlInfo.IsUrl)
            .Select(urlInfo => new DocumentUrlInfo { Culture = urlInfo.Culture, Url = urlInfo.Text })
            .ToArray();
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
