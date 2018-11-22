using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    public interface ITagQuery
    {
        /// <summary>
        /// Returns all content that is tagged with the specified tag value and optional tag group
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        IEnumerable<IPublishedContent> GetContentByTag(string tag, string tagGroup = null);

        /// <summary>
        /// Returns all content that has been tagged with any tag in the specified group
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        IEnumerable<IPublishedContent> GetContentByTagGroup(string tagGroup);

        /// <summary>
        /// Returns all Media that is tagged with the specified tag value and optional tag group
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        IEnumerable<IPublishedContent> GetMediaByTag(string tag, string tagGroup = null);

        /// <summary>
        /// Returns all Media that has been tagged with any tag in the specified group
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        IEnumerable<IPublishedContent> GetMediaByTagGroup(string tagGroup);

        /// <summary>
        /// Get every tag stored in the database (with optional group)
        /// </summary>
        IEnumerable<TagModel> GetAllTags(string group = null);

        /// <summary>
        /// Get all tags for content items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        IEnumerable<TagModel> GetAllContentTags(string group = null);

        /// <summary>
        /// Get all tags for media items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        IEnumerable<TagModel> GetAllMediaTags(string group = null);

        /// <summary>
        /// Get all tags for member items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        IEnumerable<TagModel> GetAllMemberTags(string group = null);

        /// <summary>
        /// Returns all tags attached to a property by entity id
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        IEnumerable<TagModel> GetTagsForProperty(int contentId, string propertyTypeAlias, string tagGroup = null);

        /// <summary>
        /// Returns all tags attached to an entity (content, media or member) by entity id
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        IEnumerable<TagModel> GetTagsForEntity(int contentId, string tagGroup = null);
    }
}