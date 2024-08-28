using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.HybridCache.Persistence;

internal interface IDatabaseCacheRepository
{
    void DeleteContentItem(int id);

    Task<ContentCacheNode?> GetContentSource(int id, bool preview = false);

    Task<ContentCacheNode?> GetMediaSource(int id);

    // TODO: Refactor to use by key.
    IEnumerable<ContentCacheNode> GetContentByContentTypeId(IEnumerable<int>? ids);

    /// <summary>
    ///     Refreshes the nucache database row for the given cache node />
    /// </summary>
    void RefreshContent(ContentCacheNode contentCacheNode, PublishedState publishedState);

    /// <summary>
    ///     Refreshes the nucache database row for the given cache node />
    /// </summary>
    void RefreshMedia(ContentCacheNode contentCacheNode);

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

    bool VerifyContentDbCache();

    bool VerifyMediaDbCache();
}
