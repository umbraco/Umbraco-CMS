using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
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
    private readonly IDocumentUrlService _documentUrlService;

    [Obsolete("Use the non-obsolete constructor. This will be removed in Umbraco 16")]
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
    :this(
        publishedRouter,
        umbracoContextAccessor,
        languageService,
        localizedTextService,
        contentService,
        variationContextAccessor,
        loggerFactory,
        uriUtility,
        publishedUrlProvider,
        StaticServiceProvider.Instance.GetRequiredService<IDocumentUrlService>())
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
        IDocumentUrlService documentUrlService)
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
        _documentUrlService = documentUrlService;
    }

    public async Task<IEnumerable<DocumentUrlInfo>> CreateUrlsAsync(IContent content)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();

        IEnumerable<UrlInfo> urlInfos = await _documentUrlService.ListUrlsAsync(content.Key);

        //TODO replace with documentUrlService
        // IEnumerable<UrlInfo> urlInfos = await content.GetContentUrlsAsync(
        //     _publishedRouter,
        //     umbracoContext,
        //     _languageService,
        //     _localizedTextService,
        //     _contentService,
        //     _variationContextAccessor,
        //     _loggerFactory.CreateLogger<IContent>(),
        //     _uriUtility,
        //     _publishedUrlProvider);

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
