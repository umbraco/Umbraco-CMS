using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides an implementation of <see cref="IContentFinder" /> that handles page nice URLs.
/// </summary>
/// <remarks>
///     <para>Handles <c>/foo/bar</c> where <c>/foo/bar</c> is the nice URL of a document.</para>
/// </remarks>
public class ContentFinderByUrl : IContentFinder
{
    private readonly ILogger<ContentFinderByUrl> _logger;
    private readonly IDocumentUrlService _documentUrlService;

    [Obsolete("Use non-obsoleted constructor. This will be removed in Umbraco 16.")]
    public ContentFinderByUrl(ILogger<ContentFinderByUrl> logger, IUmbracoContextAccessor umbracoContextAccessor)
    : this(logger, umbracoContextAccessor, StaticServiceProvider.Instance.GetRequiredService<IDocumentUrlService>())
    {

    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrl" /> class.
    /// </summary>
    public ContentFinderByUrl(ILogger<ContentFinderByUrl> logger, IUmbracoContextAccessor umbracoContextAccessor, IDocumentUrlService documentUrlService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _documentUrlService = documentUrlService;
        UmbracoContextAccessor =
            umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
    }

    /// <summary>
    ///     Gets the <see cref="IUmbracoContextAccessor" />
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

        if (docreq == null)
        {
            throw new ArgumentNullException(nameof(docreq));
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Test route {Route}", route);
        }


        // TODO find better way to strip the id from the route
        var documentKey = _documentUrlService.GetDocumentKeyByRouteAsync(docreq.Domain is null ? route : route.Substring(docreq.Domain.ContentId.ToString().Length), docreq.Culture ?? "en-US", docreq.Domain?.ContentId).GetAwaiter().GetResult(); //TODO proper async and default culture

        IPublishedContent? node =
            umbracoContext.Content?.GetByRoute(umbracoContext.InPreviewMode, route, culture: docreq.Culture);
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
