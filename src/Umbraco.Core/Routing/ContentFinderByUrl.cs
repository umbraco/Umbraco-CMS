using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrl" /> class.
    /// </summary>
    public ContentFinderByUrl(ILogger<ContentFinderByUrl> logger, IUmbracoContextAccessor umbracoContextAccessor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
