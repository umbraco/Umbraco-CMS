using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Infrastructure.HybridCache.Persistence;

internal interface IDatabaseCacheRepository
{
    Task DeleteContentItemAsync(int id);

    Task<ContentCacheNode?> GetContentSourceAsync(Guid key, bool preview = false);

    Task<ContentCacheNode?> GetMediaSourceAsync(Guid key);


    IEnumerable<ContentCacheNode> GetContentByContentTypeKey(IEnumerable<Guid> keys, ContentCacheDataSerializerEntityType entityType);

    /// <summary>
    /// Gets all content keys of specific document types
    /// </summary>
    /// <param name="keys">The document types to find content using.</param>
    /// <returns>The keys of all content use specific document types.</returns>
    IEnumerable<Guid> GetDocumentKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false);

    /// <summary>
    ///     Refreshes the nucache database row for the given cache node />
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task RefreshContentAsync(ContentCacheNode contentCacheNode, PublishedState publishedState);

    /// <summary>
    ///     Refreshes the nucache database row for the given cache node />
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task RefreshMediaAsync(ContentCacheNode contentCacheNode);

    /// <summary>
    ///     Rebuilds the caches for content, media and/or members based on the content type ids specified
    /// </summary>
    /// <param name="contentTypeIds">
    ///     If not null will process content for the matching content types, if empty will process all
    ///     content
    /// </param>
    /// <param name="mediaTypeIds">
    ///     If not null will process content for the matching media types, if empty will process all
    ///     media
    /// </param>
    /// <param name="memberTypeIds">
    ///     If not null will process content for the matching members types, if empty will process all
    ///     members
    /// </param>
    void Rebuild(
        IReadOnlyCollection<int>? contentTypeIds = null,
        IReadOnlyCollection<int>? mediaTypeIds = null,
        IReadOnlyCollection<int>? memberTypeIds = null);

    /// <summary>
    ///     Verifies the content cache by asserting that every document should have a corresponding row for edited properties and if published,
    ///     may have a corresponding row for published properties
    /// </summary>
    bool VerifyContentDbCache();

    /// <summary>
    ///     Rebuilds the caches for content, media and/or members based on the content type ids specified
    /// </summary>
    bool VerifyMediaDbCache();

    Task<IEnumerable<Guid>> GetContentKeysAsync(Guid nodeObjectType);
}
