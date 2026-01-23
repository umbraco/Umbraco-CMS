using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Extensions;
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
    private readonly IDocumentUrlAliasService _documentUrlAliasService;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrlAlias" /> class.
    /// </summary>
    // TODO (V18): Remove this constructor and the unused parameters from the remaining one. They are only retained to avoid
    // an ambiguous constructor error. Also removed the internal "Simplified constructor for testing purposes".
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ContentFinderByUrlAlias(
        ILogger<ContentFinderByUrlAlias> logger,
        IPublishedValueFallback publishedValueFallback,
        IUmbracoContextAccessor umbracoContextAccessor,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IPublishedContentStatusFilteringService publishedContentStatusFilteringService)
        : this(
            logger,
            publishedValueFallback,
            umbracoContextAccessor,
            documentNavigationQueryService,
            publishedContentStatusFilteringService,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentUrlAliasService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrlAlias" /> class.
    /// </summary>
    public ContentFinderByUrlAlias(
        ILogger<ContentFinderByUrlAlias> logger,
#pragma warning disable IDE0060 // Remove unused parameter
        IPublishedValueFallback publishedValueFallback,
#pragma warning restore IDE0060 // Remove unused parameter
        IUmbracoContextAccessor umbracoContextAccessor,
        IDocumentNavigationQueryService documentNavigationQueryService,
#pragma warning disable IDE0060 // Remove unused parameter
        IPublishedContentStatusFilteringService publishedContentStatusFilteringService,
#pragma warning restore IDE0060 // Remove unused parameter
        IDocumentUrlAliasService documentUrlAliasService,
        IIdKeyMap idKeyMap)
    {
        _logger = logger;
        _umbracoContextAccessor = umbracoContextAccessor;
        _documentNavigationQueryService = documentNavigationQueryService;
        _documentUrlAliasService = documentUrlAliasService;
        _idKeyMap = idKeyMap;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrlAlias" /> class.
    /// </summary>
    /// <remarks>Simplified constructor for testing purposes.</remarks>
    internal ContentFinderByUrlAlias(
        ILogger<ContentFinderByUrlAlias> logger,
        IUmbracoContextAccessor umbracoContextAccessor,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IDocumentUrlAliasService documentUrlAliasService,
        IIdKeyMap idKeyMap)
    {
        _logger = logger;
        _umbracoContextAccessor = umbracoContextAccessor;
        _documentNavigationQueryService = documentNavigationQueryService;
        _documentUrlAliasService = documentUrlAliasService;
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

        var normalizedAlias = _documentUrlAliasService.NormalizeAlias(alias);

        if (string.IsNullOrEmpty(normalizedAlias))
        {
            return null;
        }

        // Get all matching document keys for the alias
        IEnumerable<Guid> documentKeys = _documentUrlAliasService.GetDocumentKeysByAliasAsync(
            normalizedAlias,
            culture).GetAwaiter().GetResult();

        Guid? matchingKey = null;

        // Convert domain root ID to Guid for scoping.
        Guid? domainRootKey = null;
        if (rootNodeId > 0)
        {
            Attempt<Guid> attempt = _idKeyMap.GetKeyForId(rootNodeId, UmbracoObjectTypes.Document);
            domainRootKey = attempt.Success ? attempt.Result : null;
        }

        // If we have a domain root, find the first document that's under that domain.
        if (domainRootKey.HasValue)
        {
            foreach (Guid documentKey in documentKeys)
            {
                if (IsDocumentUnderDomainRoot(documentKey, domainRootKey.Value))
                {
                    matchingKey = documentKey;
                    break;
                }
            }

            // If under a domain, and no match found, return null.
            if (matchingKey == null)
            {
                return null;
            }
        }
        else
        {
            // Use first match if no domain).
            matchingKey = documentKeys.FirstOrDefault();
        }

        return matchingKey != default ? cache.GetById(matchingKey.Value) : null;
    }

    private bool IsDocumentUnderDomainRoot(Guid documentKey, Guid domainRootKey)
    {
        // Document is the domain root itself
        if (documentKey == domainRootKey)
        {
            return true;
        }

        // Check if document is a descendant of the domain root
        if (_documentNavigationQueryService.TryGetAncestorsKeys(documentKey, out IEnumerable<Guid> ancestorKeys))
        {
            return ancestorKeys.Contains(domainRootKey);
        }

        return false;
    }
}
