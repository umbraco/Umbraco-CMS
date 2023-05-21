using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Persistence;

/// <summary>
///     Defines a data source for NuCache.
/// </summary>
public interface INuCacheContentService
{
    /// <summary>
    ///     Used during startup to see if the configured serialized is different from the persisted serialize type.
    ///     If they are different, this will rebuild the nucache DB table with the configured serializer.
    /// </summary>
    void RebuildDatabaseCacheIfSerializerChanged();

    // TODO: For these required sort orders, would sorting on Path 'just work'?
    ContentNodeKit GetContentSource(int id);

    /// <summary>
    ///     Returns all content ordered by level + sortOrder
    /// </summary>
    /// <remarks>
    ///     MUST be ordered by level + parentId + sortOrder!
    /// </remarks>
    IEnumerable<ContentNodeKit> GetAllContentSources();

    /// <summary>
    ///     Returns branch for content ordered by level + sortOrder
    /// </summary>
    /// <remarks>
    ///     MUST be ordered by level + parentId + sortOrder!
    /// </remarks>
    IEnumerable<ContentNodeKit> GetBranchContentSources(int id);

    /// <summary>
    ///     Returns content by Ids ordered by level + sortOrder
    /// </summary>
    /// <remarks>
    ///     MUST be ordered by level + parentId + sortOrder!
    /// </remarks>
    IEnumerable<ContentNodeKit> GetTypeContentSources(IEnumerable<int>? ids);

    ContentNodeKit GetMediaSource(int id);

    /// <summary>
    ///     Returns all media ordered by level + sortOrder
    /// </summary>
    /// <remarks>
    ///     MUST be ordered by level + parentId + sortOrder!
    /// </remarks>
    IEnumerable<ContentNodeKit> GetAllMediaSources();

    /// <summary>
    ///     Returns branch for media ordered by level + sortOrder
    /// </summary>
    /// <remarks>
    ///     MUST be ordered by level + parentId + sortOrder!
    /// </remarks>
    IEnumerable<ContentNodeKit> GetBranchMediaSources(int id); // must order by level, sortOrder

    /// <summary>
    ///     Returns media by Ids ordered by level + sortOrder
    /// </summary>
    /// <remarks>
    ///     MUST be ordered by level + parentId + sortOrder!
    /// </remarks>
    IEnumerable<ContentNodeKit> GetTypeMediaSources(IEnumerable<int> ids);

    void DeleteContentItem(IContentBase item);

    void DeleteContentItems(IEnumerable<IContentBase> items);

    /// <summary>
    ///     Refreshes the nucache database row for the <see cref="IContent" />
    /// </summary>
    void RefreshContent(IContent content);

    /// <summary>
    ///     Refreshes the nucache database row for the <see cref="IMedia" />
    /// </summary>
    void RefreshMedia(IMedia media);

    /// <summary>
    ///     Refreshes the nucache database row for the <see cref="IMember" />
    /// </summary>
    void RefreshMember(IMember member);

    /// <summary>
    ///     Rebuilds the database caches for content, media and/or members based on the content type ids specified
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

    bool VerifyMemberDbCache();
}
