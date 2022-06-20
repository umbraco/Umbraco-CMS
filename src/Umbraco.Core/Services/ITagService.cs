using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Tag service to query for tags in the tags db table. The tags returned are only relevant for published content &
///     saved media or members
/// </summary>
/// <remarks>
///     If there is unpublished content with tags, those tags will not be contained.
///     This service does not contain methods to query for content, media or members based on tags, those methods will be added
///     to the content, media and member services respectively.
/// </remarks>
public interface ITagService : IService
{
    /// <summary>
    ///     Gets a tagged entity.
    /// </summary>
    TaggedEntity? GetTaggedEntityById(int id);

    /// <summary>
    ///     Gets a tagged entity.
    /// </summary>
    TaggedEntity? GetTaggedEntityByKey(Guid key);

    /// <summary>
    ///     Gets all documents tagged with any tag in the specified group.
    /// </summary>
    IEnumerable<TaggedEntity> GetTaggedContentByTagGroup(string group, string? culture = null);

    /// <summary>
    ///     Gets all documents tagged with the specified tag.
    /// </summary>
    IEnumerable<TaggedEntity> GetTaggedContentByTag(string tag, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all media tagged with any tag in the specified group.
    /// </summary>
    IEnumerable<TaggedEntity> GetTaggedMediaByTagGroup(string group, string? culture = null);

    /// <summary>
    ///     Gets all media tagged with the specified tag.
    /// </summary>
    IEnumerable<TaggedEntity> GetTaggedMediaByTag(string tag, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all members tagged with any tag in the specified group.
    /// </summary>
    IEnumerable<TaggedEntity> GetTaggedMembersByTagGroup(string group, string? culture = null);

    /// <summary>
    ///     Gets all members tagged with the specified tag.
    /// </summary>
    IEnumerable<TaggedEntity> GetTaggedMembersByTag(string tag, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags.
    /// </summary>
    IEnumerable<ITag> GetAllTags(string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all document tags.
    /// </summary>
    IEnumerable<ITag> GetAllContentTags(string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all media tags.
    /// </summary>
    IEnumerable<ITag> GetAllMediaTags(string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all member tags.
    /// </summary>
    IEnumerable<ITag> GetAllMemberTags(string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity via a property.
    /// </summary>
    IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity.
    /// </summary>
    IEnumerable<ITag> GetTagsForEntity(int contentId, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity via a property.
    /// </summary>
    IEnumerable<ITag> GetTagsForProperty(Guid contentId, string propertyTypeAlias, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity.
    /// </summary>
    IEnumerable<ITag> GetTagsForEntity(Guid contentId, string? group = null, string? culture = null);
}
