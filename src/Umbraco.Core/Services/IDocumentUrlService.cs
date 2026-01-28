using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Defines operations for handling document URLs.
/// </summary>
public interface IDocumentUrlService
{
    /// <summary>
    /// Gets a value indicating whether the service has been initialized.
    /// </summary>
    // TODO (V18): Remove the default implementation.
    bool IsInitialized => false;

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
    /// Gets a single URL segment from a document key and culture. Preview urls are returned if isDraft is true.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns>A URL segment for the document.</returns>
    /// <remarks>If more than one segment is available, the first retrieved and indicated as primary will be returned.</remarks>
    string? GetUrlSegment(Guid documentKey, string culture, bool isDraft);

    /// <summary>
    /// Gets the URL segments from a document key and culture. Preview urls are returned if isDraft is true.
    /// </summary>
    /// <param name="documentKey">The key of the document.</param>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns>The URL segments for the document.</returns>
    IEnumerable<string> GetUrlSegments(Guid documentKey, string culture, bool isDraft)
        => throw new NotImplementedException();

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
    /// Gets a document key by <see cref="Uri" />.
    /// </summary>
    /// <param name="uri">The uniform resource identifier.</param>
    /// <param name="isDraft">Whether to get the url of the draft or published document.</param>
    /// <returns>The document key, or null if not found.</returns>
    Guid? GetDocumentKeyByUri(Uri uri, bool isDraft) => throw new NotImplementedException(); // TODO (V19): Remove default implementation.

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
