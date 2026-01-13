using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides an implementation of <see cref="IContentFinder" /> that handles page aliases.
/// </summary>
/// <remarks>
///     <para>
///         Handles <c>/just/about/anything</c> where <c>/just/about/anything</c> is contained in the
///         <c>umbracoUrlAlias</c> property of a document.
///     </para>
///     <para>The alias is the full path to the document. There can be more than one alias, separated by commas.</para>
/// </remarks>
public class ContentFinderByUrlAlias : IContentFinder
{
    private readonly ILogger<ContentFinderByUrlAlias> _logger;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IDocumentAliasService _documentAliasService;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrlAlias" /> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public ContentFinderByUrlAlias(
        ILogger<ContentFinderByUrlAlias> logger,
        IPublishedValueFallback publishedValueFallback,
        IUmbracoContextAccessor umbracoContextAccessor,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IPublishedContentStatusFilteringService publishedContentStatusFilteringService)
        : this(
            logger,
            umbracoContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentAliasService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrlAlias" /> class.
    /// </summary>
    public ContentFinderByUrlAlias(
        ILogger<ContentFinderByUrlAlias> logger,
        IUmbracoContextAccessor umbracoContextAccessor,
        IDocumentAliasService documentAliasService,
        IIdKeyMap idKeyMap)
    {
        _logger = logger;
        _umbracoContextAccessor = umbracoContextAccessor;
        _documentAliasService = documentAliasService;
        _idKeyMap = idKeyMap;
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

        IPublishedContent? node = null;

        // no alias if "/"
        if (frequest.Uri.AbsolutePath != "/")
        {
            node = FindContentByAlias(
                umbracoContext.Content,
                frequest.Domain?.ContentId ?? 0,
                frequest.Culture,
                frequest.AbsolutePathDecoded);

            if (node != null)
            {
                frequest.SetPublishedContent(node);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(
                        "Path '{UriAbsolutePath}' is an alias for id={PublishedContentId}",
                        frequest.Uri.AbsolutePath,
                        node.Id);
                }
            }
        }

        return Task.FromResult(node != null);
    }

    private IPublishedContent? FindContentByAlias(
        IPublishedContentCache? cache,
        int rootNodeId,
        string? culture,
        string alias)
    {
        if (cache is null)
        {
            return null;
        }

        // Normalize alias (same as service)
        var normalizedAlias = alias
            .TrimStart('/')
            .TrimEnd('/')
            .ToLowerInvariant();

        if (string.IsNullOrEmpty(normalizedAlias))
        {
            return null;
        }

        // Convert domain root ID to Guid for scoping
        Guid? domainRootKey = null;
        if (rootNodeId > 0)
        {
            Attempt<Guid> attempt = _idKeyMap.GetKeyForId(rootNodeId, UmbracoObjectTypes.Document);
            domainRootKey = attempt.Success ? attempt.Result : null;
        }

        // O(1) lookup instead of O(n) tree traversal
        Guid? documentKey = _documentAliasService.GetDocumentKeyByAlias(
            normalizedAlias,
            culture,
            domainRootKey);

        return documentKey.HasValue ? cache.GetById(documentKey.Value) : null;
    }
}
