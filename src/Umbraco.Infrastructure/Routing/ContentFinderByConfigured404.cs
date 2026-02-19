using System.Globalization;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides an implementation of <see cref="IContentFinder" /> that runs the legacy 404 logic.
/// </summary>
public class ContentFinderByConfigured404 : IContentLastChanceFinder
{
    private readonly IEntityService _entityService;
    private readonly IExamineManager _examineManager;
    private readonly ILogger<ContentFinderByConfigured404> _logger;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IMediaNavigationQueryService _mediaNavigationQueryService;
    private ContentSettings _contentSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByConfigured404" /> class.
    /// </summary>
    public ContentFinderByConfigured404(
        ILogger<ContentFinderByConfigured404> logger,
        IEntityService entityService,
        IOptionsMonitor<ContentSettings> contentSettings,
        IExamineManager examineManager,
        IVariationContextAccessor variationContextAccessor,
        IUmbracoContextAccessor umbracoContextAccessor,
        IDocumentUrlService documentUrlService,
        IPublishedContentCache publishedContentCache,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IMediaNavigationQueryService mediaNavigationQueryService)
    {
        _logger = logger;
        _entityService = entityService;
        _contentSettings = contentSettings.CurrentValue;
        _examineManager = examineManager;
        _variationContextAccessor = variationContextAccessor;
        _umbracoContextAccessor = umbracoContextAccessor;
        _documentUrlService = documentUrlService;
        _publishedContentCache = publishedContentCache;
        _documentNavigationQueryService = documentNavigationQueryService;
        _mediaNavigationQueryService = mediaNavigationQueryService;

        contentSettings.OnChange(x => _contentSettings = x);
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public ContentFinderByConfigured404(
        ILogger<ContentFinderByConfigured404> logger,
        IEntityService entityService,
        IOptionsMonitor<ContentSettings> contentSettings,
        IExamineManager examineManager,
        IVariationContextAccessor variationContextAccessor,
        IUmbracoContextAccessor umbracoContextAccessor,
        IDocumentUrlService documentUrlService,
        IPublishedContentCache publishedContentCache,
        IDocumentNavigationQueryService documentNavigationQueryService)
        : this(
            logger,
            entityService,
            contentSettings,
            examineManager,
            variationContextAccessor,
            umbracoContextAccessor,
            documentUrlService,
            publishedContentCache,
            documentNavigationQueryService,
            StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>())
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ContentFinderByConfigured404(
        ILogger<ContentFinderByConfigured404> logger,
        IEntityService entityService,
        IOptionsMonitor<ContentSettings> contentSettings,
        IExamineManager examineManager,
        IVariationContextAccessor variationContextAccessor,
        IUmbracoContextAccessor umbracoContextAccessor)
        : this(
            logger,
            entityService,
            contentSettings,
            examineManager,
            variationContextAccessor,
            umbracoContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentUrlService>(),
            StaticServiceProvider.Instance.GetRequiredService<IPublishedContentCache>(),
            StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>())
    {
    }

    /// <summary>
    ///     Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
    /// </summary>
    /// <param name="frequest">The <c>PublishedRequest</c>.</param>
    /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
    public Task<bool> TryFindContent(IPublishedRequestBuilder frequest)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
        {
            return Task.FromResult(false);
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Looking for a page to handle 404.");
        }

        int? domainContentId = null;

        // try to find a culture as best as we can
        var errorCulture = CultureInfo.CurrentUICulture.Name;
        if (frequest.Domain != null)
        {
            errorCulture = frequest.Domain.Culture;
            domainContentId = frequest.Domain.ContentId;
        }
        else
        {
            var route = frequest.AbsolutePathDecoded;
            var pos = route.LastIndexOf('/');
            IPublishedContent? node = null;
            while (pos > 1)
            {
                route = route.Substring(0, pos);
                Guid? keyByRoute = _documentUrlService.GetDocumentKeyByRoute(route, frequest.Culture, null, false);
                if (keyByRoute is not null)
                {
                    node = _publishedContentCache.GetById(keyByRoute.Value);
                }

                if (node is not null)
                {
                    break;
                }

                pos = route.LastIndexOf('/');
            }

            if (node != null)
            {
                Domain? d = DomainUtilities.FindWildcardDomainInPath(
                    umbracoContext.Domains?.GetAll(true), node.Path, null);
                if (d != null)
                {
                    errorCulture = d.Culture;
                }
            }
        }

        var error404 = NotFoundHandlerHelper.GetCurrentNotFoundPageId(
            _contentSettings.Error404Collection.ToArray(),
            _entityService,
            new PublishedContentQuery(_variationContextAccessor, _examineManager, umbracoContext.Content!, umbracoContext.Media, _documentNavigationQueryService, _mediaNavigationQueryService),
            errorCulture,
            domainContentId);

        IPublishedContent? content = null;

        if (error404.HasValue)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Got id={ErrorNodeId}.", error404.Value);
            }

            content = umbracoContext.Content?.GetById(error404.Value);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(content == null
                    ? "Could not find content with that id."
                    : "Found corresponding content.");
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Got nothing.");
            }
        }

        frequest?
            .SetPublishedContent(content)
            .SetIs404();

        return Task.FromResult(content != null);
    }
}
