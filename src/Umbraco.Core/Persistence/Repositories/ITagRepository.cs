using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface ITagRepository : IReadWriteQueryRepository<int, ITag>
{
    #region Assign and Remove Tags

    /// <summary>
    ///     Assign tags to a content property.
    /// </summary>
    /// <param name="contentId">The identifier of the content item.</param>
    /// <param name="propertyTypeId">The identifier of the property type.</param>
    /// <param name="tags">The tags to assign.</param>
    /// <param name="replaceTags">A value indicating whether to replace already assigned tags.</param>
    /// <remarks>
    ///     <para>
    ///         When <paramref name="replaceTags" /> is false, the tags specified in <paramref name="tags" /> are added to
    ///         those already assigned.
    ///     </para>
    ///     <para>
    ///         When <paramref name="tags" /> is empty and <paramref name="replaceTags" /> is true, all assigned tags are
    ///         removed.
    ///     </para>
    /// </remarks>
    // TODO: replaceTags is used as 'false' in tests exclusively - should get rid of it
    void Assign(int contentId, int propertyTypeId, IEnumerable<ITag> tags, bool replaceTags = true);

    /// <summary>
    ///     Removes assigned tags from a content property.
    /// </summary>
    /// <param name="contentId">The identifier of the content item.</param>
    /// <param name="propertyTypeId">The identifier of the property type.</param>
    /// <param name="tags">The tags to remove.</param>
    void Remove(int contentId, int propertyTypeId, IEnumerable<ITag> tags);

    /// <summary>
    ///     Removes all assigned tags from a content item.
    /// </summary>
    /// <param name="contentId">The identifier of the content item.</param>
    void RemoveAll(int contentId);

    /// <summary>
    ///     Removes all assigned tags from a content property.
    /// </summary>
    /// <param name="contentId">The identifier of the content item.</param>
    /// <param name="propertyTypeId">The identifier of the property type.</param>
    void RemoveAll(int contentId, int propertyTypeId);

    #endregion

    #region Queries

    /// <summary>
    ///     Gets a tagged entity.
    /// </summary>
    TaggedEntity? GetTaggedEntityByKey(Guid key);

    /// <summary>
    ///     Gets a tagged entity.
    /// </summary>
    TaggedEntity? GetTaggedEntityById(int id);

    /// Gets all entities of a type, tagged with any tag in the specified group.
    IEnumerable<TaggedEntity> GetTaggedEntitiesByTagGroup(TaggableObjectTypes objectType, string group, string? culture = null);

    /// <summary>
    ///     Gets all entities of a type, tagged with the specified tag.
    /// </summary>
    IEnumerable<TaggedEntity> GetTaggedEntitiesByTag(TaggableObjectTypes objectType, string tag, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags for an entity type.
    /// </summary>
    IEnumerable<ITag> GetTagsForEntityType(TaggableObjectTypes objectType, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity.
    /// </summary>
    IEnumerable<ITag> GetTagsForEntity(int contentId, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity.
    /// </summary>
    IEnumerable<ITag> GetTagsForEntity(Guid contentId, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity via a property.
    /// </summary>
    IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity via a property.
    /// </summary>
    IEnumerable<ITag> GetTagsForProperty(Guid contentId, string propertyTypeAlias, string? group = null, string? culture = null);

    #endregion
}
