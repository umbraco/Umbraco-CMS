namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Queues content type, media type, and member type IDs for deferred background search reindexing with de-duplication.
/// </summary>
public interface IDeferredSearchReindexService
{
    /// <summary>
    ///     Queues the specified content type IDs for a deferred search reindex.
    /// </summary>
    /// <param name="contentTypeIds">The content type IDs to reindex.</param>
    void QueueContentTypeReindex(IReadOnlyCollection<int> contentTypeIds);

    /// <summary>
    ///     Queues the specified media type IDs for a deferred search reindex.
    /// </summary>
    /// <param name="mediaTypeIds">The media type IDs to reindex.</param>
    void QueueMediaTypeReindex(IReadOnlyCollection<int> mediaTypeIds);

    /// <summary>
    ///     Queues the specified member type IDs for a deferred search reindex.
    /// </summary>
    /// <param name="memberTypeIds">The member type IDs to reindex.</param>
    void QueueMemberTypeReindex(IReadOnlyCollection<int> memberTypeIds);
}
