using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ITagsRepository : IRepository<int, ITag>
    {

        /// <summary>
        /// Returns all tags that exist on the content item - Content/Media/Member
        /// </summary>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <returns></returns>
        IEnumerable<ITag> GetTagsForContent(int contentId);

        /// <summary>
        /// Returns all tags that exist on the content item for the property specified - Content/Media/Member
        /// </summary>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="propertyAlias">The property alias to get tags for</param>
        /// <returns></returns>
        IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyAlias);

        /// <summary>
        /// Assigns the given tags to a content item's property
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="tags">The tags to assign</param>
        /// <param name="replaceTags">
        /// If set to true, this will replace all tags with the given tags, 
        /// if false this will append the tags that already exist for the content item
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// This can also be used to remove all tags from a property by specifying replaceTags = true and an empty tag list.
        /// </remarks>
        void AssignTagsToProperty(int contentId, string propertyAlias, IEnumerable<ITag> tags, bool replaceTags);

        /// <summary>
        /// Removes any of the given tags from the property association
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="tags">The tags to remove from the property</param>
        void RemoveTagsFromProperty(int contentId, string propertyAlias, IEnumerable<ITag> tags);
    }
}