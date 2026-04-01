using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Infrastructure.HybridCache.Persistence;

/// <summary>
/// Defines a repository for persisting content cache data.
/// </summary>
internal interface IDatabaseCacheRepository
{
    // --- Document methods ---

    /// <summary>
    /// Gets a single cache node for a document key.
    /// </summary>
    /// <param name="key">The document key.</param>
    /// <param name="preview">A flag indicating whether to get the draft (preview) version or the published version.</param>
    Task<ContentCacheNode?> GetDocumentSourceAsync(Guid key, bool preview = false);

    /// <summary>
    /// Gets both draft and published cache nodes for a document key in a single query.
    /// </summary>
    /// <param name="key">The document key.</param>
    /// <returns>A tuple containing the draft and published cache nodes (either may be null).</returns>
    Task<(ContentCacheNode? Draft, ContentCacheNode? Published)> GetDocumentSourceForPublishStatesAsync(Guid key);

    /// <summary>
    /// Gets a collection of cache nodes for a collection of document keys.
    /// </summary>
    /// <param name="keys">The document keys.</param>
    /// <param name="preview">A flag indicating whether to get the draft (preview) version or the published version.</param>
    Task<IEnumerable<ContentCacheNode>> GetDocumentSourcesAsync(IEnumerable<Guid> keys, bool preview = false);

    /// <summary>
    /// Refreshes the cache for the given document cache node.
    /// </summary>
    Task RefreshDocumentAsync(ContentCacheNode contentCacheNode, PublishedState publishedState);

    /// <summary>
    /// Gets all content keys of specific document types.
    /// </summary>
    /// <param name="keys">The document types to find content using.</param>
    /// <param name="published">A flag indicating whether to restrict to just published content.</param>
    /// <returns>The keys of all content use specific document types.</returns>
    IEnumerable<Guid> GetDocumentKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false);

    /// <summary>
    /// Gets content keys and their draft/published status for specific document types.
    /// </summary>
    /// <param name="contentTypeKeys">The document type keys to find content for.</param>
    /// <returns>Tuples of content key and whether the cache entry is a draft.</returns>
    IEnumerable<(Guid Key, bool IsDraft)> GetDocumentKeysWithPublishedStatus(IEnumerable<Guid> contentTypeKeys);

    // --- Media methods ---

    /// <summary>
    /// Gets a single cache node for a media key.
    /// </summary>
    Task<ContentCacheNode?> GetMediaSourceAsync(Guid key);

    /// <summary>
    /// Gets a collection of cache nodes for a collection of media keys.
    /// </summary>
    Task<IEnumerable<ContentCacheNode>> GetMediaSourcesAsync(IEnumerable<Guid> keys);

    /// <summary>
    /// Refreshes the cache row for the given media cache node.
    /// </summary>
    Task RefreshMediaAsync(ContentCacheNode contentCacheNode);

    /// <summary>
    /// Gets all media content keys for specific media types.
    /// Uses a lightweight query that avoids loading serialized data.
    /// </summary>
    /// <param name="mediaTypeKeys">The media type keys to find media for.</param>
    /// <returns>The keys of all media items using the specified media types.</returns>
    IEnumerable<Guid> GetMediaKeysByContentTypeKeys(IEnumerable<Guid> mediaTypeKeys);

    // --- Element methods ---

    /// <summary>
    /// Gets a single cache node for an element key.
    /// </summary>
    /// <param name="key">The element key.</param>
    /// <param name="preview">A flag indicating whether to get the draft (preview) version or the published version.</param>
    Task<ContentCacheNode?> GetElementSourceAsync(Guid key, bool preview = false);

    /// <summary>
    /// Gets both draft and published cache nodes for an element key in a single query.
    /// </summary>
    /// <param name="key">The element key.</param>
    /// <returns>A tuple containing the draft and published cache nodes (either may be null).</returns>
    Task<(ContentCacheNode? Draft, ContentCacheNode? Published)> GetElementSourceForPublishStatesAsync(Guid key);

    /// <summary>
    /// Gets a collection of cache nodes for a collection of element keys.
    /// </summary>
    /// <param name="keys">The element keys.</param>
    /// <param name="preview">A flag indicating whether to get the draft (preview) version or the published version.</param>
    Task<IEnumerable<ContentCacheNode>> GetElementSourcesAsync(IEnumerable<Guid> keys, bool preview = false);

    /// <summary>
    /// Refreshes the cache for the given element cache node.
    /// </summary>
    Task RefreshElementAsync(ContentCacheNode contentCacheNode, PublishedState publishedState);

    // --- Shared methods ---

    /// <summary>
    /// Deletes the specified content item from the cache database.
    /// </summary>
    Task DeleteContentItemAsync(int id);

    /// <summary>
    /// Gets a collection of cache nodes for a collection of content type keys and entity type.
    /// </summary>
    IEnumerable<ContentCacheNode> GetContentByContentTypeKey(IEnumerable<Guid> keys, ContentCacheDataSerializerEntityType entityType);

    /// <summary>
    /// Rebuilds the caches for content, media, members and/or elements based on the content type ids specified.
    /// </summary>
    /// <param name="contentTypeIds">
    ///     If not null will process content for the matching content types, if empty will process all
    ///     content.
    /// </param>
    /// <param name="mediaTypeIds">
    ///     If not null will process content for the matching media types, if empty will process all
    ///     media.
    /// </param>
    /// <param name="memberTypeIds">
    ///     If not null will process content for the matching members types, if empty will process all
    ///     members.
    /// </param>
    /// <param name="elementTypeIds">
    ///     If not null will process content for the matching element types, if empty will process all
    ///     elements.
    /// </param>
    void Rebuild(
        IReadOnlyCollection<int>? contentTypeIds = null,
        IReadOnlyCollection<int>? mediaTypeIds = null,
        IReadOnlyCollection<int>? memberTypeIds = null,
        IReadOnlyCollection<int>? elementTypeIds = null);
}
