using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface ITagQuery
{
    /// <summary>
    ///     Gets all documents tagged with the specified tag.
    /// </summary>
    IEnumerable<IPublishedContent> GetContentByTag(string tag, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all documents tagged with any tag in the specified group.
    /// </summary>
    IEnumerable<IPublishedContent> GetContentByTagGroup(string group, string? culture = null);

    /// <summary>
    ///     Gets all media tagged with the specified tag.
    /// </summary>
    IEnumerable<IPublishedContent> GetMediaByTag(string tag, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all media tagged with any tag in the specified group.
    /// </summary>
    IEnumerable<IPublishedContent> GetMediaByTagGroup(string group, string? culture = null);

    /// <summary>
    ///     Gets all tags.
    /// </summary>
    IEnumerable<TagModel?> GetAllTags(string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all document tags.
    /// </summary>
    ///   /// <remarks>
    ///     If no culture is specified, it retrieves tags with an invariant culture.
    ///     If a culture is specified, it only retrieves tags for that culture.
    ///     Use "*" to retrieve tags for all cultures.
    /// </remarks>
    IEnumerable<TagModel?> GetAllContentTags(string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all media tags.
    /// </summary>
    IEnumerable<TagModel?> GetAllMediaTags(string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all member tags.
    /// </summary>
    IEnumerable<TagModel?> GetAllMemberTags(string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity via a property.
    /// </summary>
    IEnumerable<TagModel?> GetTagsForProperty(int contentId, string propertyTypeAlias, string? group = null, string? culture = null);

    /// <summary>
    ///     Gets all tags attached to an entity.
    /// </summary>
    IEnumerable<TagModel?> GetTagsForEntity(int contentId, string? group = null, string? culture = null);
}
