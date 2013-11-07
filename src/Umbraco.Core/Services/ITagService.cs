using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Tag service to query for tags in the tags db table. The tags returned are only relavent for published content & saved media or members 
    /// </summary>
    /// <remarks>
    /// If there is unpublished content with tags, those tags will not be contained.
    /// 
    /// This service does not contain methods to query for content, media or members based on tags, those methods will be added
    /// to the content, media and member services respectively.
    /// </remarks>
    public interface ITagService : IService
    {   

        /// <summary>
        /// Get every tag stored in the database (with optional group)
        /// </summary>
        IEnumerable<ITag> GetAllTags(string group = null);

        /// <summary>
        /// Get all tags for content items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        IEnumerable<ITag> GetAllContentTags(string group = null);

        /// <summary>
        /// Get all tags for media items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        IEnumerable<ITag> GetAllMediaTags(string group = null);

        /// <summary>
        /// Get all tags for member items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        IEnumerable<ITag> GetAllMemberTags(string group = null);

        /// <summary>
        /// Returns all tags attached to a property by entity id
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string tagGroup = null);

        /// <summary>
        /// Returns all tags attached to an entity (content, media or member) by entity id
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        IEnumerable<ITag> GetTagsForEntity(int contentId, string tagGroup = null);

    }
}