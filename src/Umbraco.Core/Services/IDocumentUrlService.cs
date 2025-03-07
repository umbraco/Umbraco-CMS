using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Defines operations for handling document URLs.
/// </summary>
public interface IDocumentUrlService
{
    /// <summary>
    /// Initializes the service and ensure the content in the database is correct with the current configuration.
    /// </summary>
    /// <param name="forceEmpty">Forces an early return when we know there are no routes (i.e. on install).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task InitAsync(bool forceEmpty, CancellationToken cancellationToken);

    /// <summary>
    /// Rebuilds all document URLs.
    /// </summary>
    Task RebuildAllUrlsAsync();

    /// <summary>
    /// Gets the URL segment from a document key and culture. Preview urls are returned if isDraft is true.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns>The URL segment for the document.</returns>
    string? GetUrlSegment(Guid documentKey, string culture, bool isDraft);

    /// <summary>
    /// Creates or updates the URL segments for a single document.
    /// </summary>
    /// <param name="key">The document key.</param>
    Task CreateOrUpdateUrlSegmentsAsync(Guid key);

    /// <summary>
    /// Creates or updates the URL segments for a document and it's descendants.
    /// </summary>
    /// <param name="key">The document key.</param>
    Task CreateOrUpdateUrlSegmentsWithDescendantsAsync(Guid key);

    /// <summary>
    /// Creates or updates the URL segments for a collection of documents.
    /// </summary>
    /// <param name="documents">The document collection.</param>
    Task CreateOrUpdateUrlSegmentsAsync(IEnumerable<IContent> documents);

    /// <summary>
    /// Deletes all URLs from the cache for a collection of document keys.
    /// </summary>
    /// <param name="documentKeys">The collection of document keys.</param>
    Task DeleteUrlsFromCacheAsync(IEnumerable<Guid> documentKeys);

    /// <summary>
    /// Gets a document key by route.
    /// </summary>
    /// <param name="route">The route.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="documentStartNodeId">The document start node Id.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns>The document key, or null if not found.</returns>
    Guid? GetDocumentKeyByRoute(string route, string? culture, int? documentStartNodeId, bool isDraft);

    /// <summary>
    /// Gets all the URLs for a given content key.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    [Obsolete("This method is obsolete and will be removed in future versions. Use IPublishedUrlInfoProvider.GetAllAsync instead. Scheduled for removal in Umbraco 17.")]
    Task<IEnumerable<UrlInfo>> ListUrlsAsync(Guid contentKey);

    /// <summary>
    /// Gets the legacy route format for a document key and culture.
    /// </summary>
    /// <param name="key">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns></returns>
    string GetLegacyRouteFormat(Guid key, string? culture, bool isDraft);

    /// <summary>
    /// Gets a value indicating whether any URLs have been cached.
    /// </summary>
    bool HasAny();
}
