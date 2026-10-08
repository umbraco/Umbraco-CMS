namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
///     Queues content type cache rebuilds for deferred background processing with de-duplication.
/// </summary>
public interface IDeferredCacheRebuildService
{
    /// <summary>
    ///     Queues the specified content type IDs for a deferred database cache rebuild.
    /// </summary>
    /// <param name="contentTypeIds">The content type IDs to rebuild.</param>
    void QueueContentTypeRebuild(IReadOnlyCollection<int> contentTypeIds);

    /// <summary>
    ///     Queues the specified media type IDs for a deferred database cache rebuild.
    /// </summary>
    /// <param name="mediaTypeIds">The media type IDs to rebuild.</param>
    void QueueMediaTypeRebuild(IReadOnlyCollection<int> mediaTypeIds);

    /// <summary>
    ///     Queues the specified element type IDs for a deferred database cache rebuild.
    /// </summary>
    /// <param name="elementTypeIds">The element type IDs to rebuild.</param>
    void QueueElementTypeRebuild(IReadOnlyCollection<int> elementTypeIds);
}
