using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Search;

public interface IUmbracoIndexingHandler
{
    /// <summary>
    ///     Returns true if the indexing handler is enabled
    /// </summary>
    /// <remarks>
    ///     If this is false then there will be no data lookups executed to populate indexes
    ///     when service changes are made.
    /// </remarks>
    bool Enabled { get; }

    void ReIndexForContent(IContent sender, bool isPublished);

    void ReIndexForMember(IMember member);

    void ReIndexForMedia(IMedia sender, bool isPublished);

    /// <summary>
    ///     Deletes all documents for the content type Ids
    /// </summary>
    /// <param name="removedContentTypes"></param>
    void DeleteDocumentsForContentTypes(IReadOnlyCollection<int> removedContentTypes);

    /// <summary>
    ///     Remove an item from an index
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="keepIfUnpublished">
    ///     If true, indicates that we will only delete this item from indexes that don't support unpublished content.
    ///     If false it will delete this from all indexes regardless.
    /// </param>
    void DeleteIndexForEntity(int entityId, bool keepIfUnpublished);

    /// <summary>
    ///     Remove items from an index
    /// </summary>
    /// <param name="entityIds"></param>
    /// <param name="keepIfUnpublished">
    ///     If true, indicates that we will only delete this item from indexes that don't support unpublished content.
    ///     If false it will delete this from all indexes regardless.
    /// </param>
    void DeleteIndexForEntities(IReadOnlyCollection<int> entityIds, bool keepIfUnpublished);
}
