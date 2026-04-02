using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides an implementation of <see cref="IContentFinder" /> that handles page nice URLs.
/// </summary>
/// <remarks>
///     <para>Handles <c>/foo/bar</c> where <c>/foo/bar</c> is the nice URL of a document.</para>
/// </remarks>
public class ContentFinderByUrlNew : IContentFinder
{
    private readonly ILogger<ContentFinderByUrlNew> _logger;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IDocumentUrlService _documentUrlService;
    private WebRoutingSettings _webRoutingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrl" /> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public ContentFinderByUrlNew(
        ILogger<ContentFinderByUrlNew> logger,
        IUmbracoContextAccessor umbracoContextAccessor,
        IDocumentUrlService documentUrlService,
        IPublishedContentCache publishedContentCache)
        : this(
              logger,
              umbracoContextAccessor,
              documentUrlService,
              publishedContentCache,
              StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<WebRoutingSettings>>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrl" /> class.
    /// </summary>
    public ContentFinderByUrlNew(
        ILogger<ContentFinderByUrlNew> logger,
        IUmbracoContextAccessor umbracoContextAccessor,
        IDocumentUrlService documentUrlService,
        IPublishedContentCache publishedContentCache,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings)
    {
        _logger = logger;
        _publishedContentCache = publishedContentCache;
        _documentUrlService = documentUrlService;
        UmbracoContextAccessor = umbracoContextAccessor;

        _webRoutingSettings = webRoutingSettings.CurrentValue;
        webRoutingSettings.OnChange(x => _webRoutingSettings = x);
    }

    /// <summary>
    ///     Gets the <see cref="IUmbracoContextAccessor" />.
    /// </summary>
    protected IUmbracoContextAccessor UmbracoContextAccessor { get; }

    /// <summary>
    ///     Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
    /// </summary>
    /// <param name="frequest">The <c>PublishedRequest</c>.</param>
    /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
    public virtual Task<bool> TryFindContent(IPublishedRequestBuilder frequest)
    {
        if (!UmbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? _))
        {
            return Task.FromResult(false);
        }

        string route;
        if (frequest.Domain != null)
        {
            route = frequest.Domain.ContentId +
                    DomainUtilities.PathRelativeToDomain(frequest.Domain.Uri, frequest.AbsolutePathDecoded);
        }
        else
        {
            // If we have configured strict domain matching, and a domain has not been found for the request configured on an ancestor node,
            // do not route the content by URL.
            if (_webRoutingSettings.UseStrictDomainMatching)
            {
                return Task.FromResult(false);
            }

            // Default behaviour if strict domain matching is not enabled will be to route under the to the first root node found.
            route = frequest.AbsolutePathDecoded;
        }

        IPublishedContent? node = FindContent(frequest, route);
        return Task.FromResult(node != null);
    }

    /// <summary>
    ///     Tries to find an Umbraco document for a <c>PublishedRequest</c> and a route.
    /// </summary>
    /// <returns>The document node, or null.</returns>
    protected IPublishedContent? FindContent(IPublishedRequestBuilder docreq, string route)
    {
        if (!UmbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
        {
            return null;
        }

        ArgumentNullException.ThrowIfNull(docreq);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Test route {Route}", route);
        }

        Guid? documentKey = _documentUrlService.GetDocumentKeyByRoute(
            docreq.Domain is null ? route : route[docreq.Domain.ContentId.ToString().Length..],
            docreq.Culture,
            docreq.Domain?.ContentId,
            umbracoContext.InPreviewMode);


        IPublishedContent? node = null;
        if (documentKey.HasValue)
        {
            node = _publishedContentCache.GetById(umbracoContext.InPreviewMode, documentKey.Value);
        }

        if (node != null)
        {
            docreq.SetPublishedContent(node);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Got content, id={NodeId}", node.Id);
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No match.");
            }
        }

        return node;
    }
}
