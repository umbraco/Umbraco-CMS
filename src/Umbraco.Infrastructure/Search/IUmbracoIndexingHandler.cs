using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Search;

/// <summary>
/// Represents a handler that defines methods for managing indexing operations related to Umbraco content and data.
/// </summary>
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

    /// <summary>
    /// Re-indexes the specified content item in the search index.
    /// </summary>
    /// <param name="sender">The <see cref="IContent"/> instance to re-index.</param>
    /// <param name="isPublished">True if the content item is published; otherwise, false.</param>
    void ReIndexForContent(IContent sender, bool isPublished);

    /// <summary>
    /// Re-indexes the specified <see cref="IMember"/> in the search index.
    /// </summary>
    /// <param name="member">The <see cref="IMember"/> instance to re-index.</param>
    void ReIndexForMember(IMember member);

    /// <summary>
    /// Re-indexes the specified media item in the search index.
    /// </summary>
    /// <param name="sender">The <see cref="IMedia"/> item to re-index.</param>
    /// <param name="isPublished">True if the media item is published; otherwise, false.</param>
    void ReIndexForMedia(IMedia sender, bool isPublished);

    /// <summary>
    ///     Removes any content that is flagged as protected
    /// </summary>
    void RemoveProtectedContent();

    /// <summary>
    ///     Deletes all documents for the content type Ids
    /// </summary>
    /// <param name="removedContentTypes">The content type IDs whose documents should be deleted.</param>
    void DeleteDocumentsForContentTypes(IReadOnlyCollection<int> removedContentTypes);

    /// <summary>
    ///     Remove an item from an index
    /// </summary>
    /// <param name="entityId">The entity ID to remove from the index.</param>
    /// <param name="keepIfUnpublished">
    ///     If true, indicates that we will only delete this item from indexes that don't support unpublished content.
    ///     If false it will delete this from all indexes regardless.
    /// </param>
    void DeleteIndexForEntity(int entityId, bool keepIfUnpublished);

    /// <summary>
    ///     Remove items from an index
    /// </summary>
    /// <param name="entityIds">The entity IDs to remove from the index.</param>
    /// <param name="keepIfUnpublished">
    ///     If true, indicates that we will only delete this item from indexes that don't support unpublished content.
    ///     If false it will delete this from all indexes regardless.
    /// </param>
    void DeleteIndexForEntities(IReadOnlyCollection<int> entityIds, bool keepIfUnpublished);
}
