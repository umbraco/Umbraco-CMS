using System;
using System.Collections.Generic;
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

        TaggedEntity GetTaggedEntityById(int id);
        TaggedEntity GetTaggedEntityByKey(Guid key);

        /// <summary>
        /// Gets tagged Content by a specific 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Content, not the actual Content item.</remarks>
        /// <param name="tagGroup">Name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        IEnumerable<TaggedEntity> GetTaggedContentByTagGroup(string tagGroup);

        /// <summary>
        /// Gets tagged Content by a specific 'Tag' and optional 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Content, not the actual Content item.</remarks>
        /// <param name="tag">Tag</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        IEnumerable<TaggedEntity> GetTaggedContentByTag(string tag, string tagGroup = null);

        /// <summary>
        /// Gets tagged Media by a specific 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Media, not the actual Media item.</remarks>
        /// <param name="tagGroup">Name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        IEnumerable<TaggedEntity> GetTaggedMediaByTagGroup(string tagGroup);

        /// <summary>
        /// Gets tagged Media by a specific 'Tag' and optional 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Media, not the actual Media item.</remarks>
        /// <param name="tag">Tag</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        IEnumerable<TaggedEntity> GetTaggedMediaByTag(string tag, string tagGroup = null);

        /// <summary>
        /// Gets tagged Members by a specific 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Member, not the actual Member item.</remarks>
        /// <param name="tagGroup">Name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        IEnumerable<TaggedEntity> GetTaggedMembersByTagGroup(string tagGroup);

        /// <summary>
        /// Gets tagged Members by a specific 'Tag' and optional 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Member, not the actual Member item.</remarks>
        /// <param name="tag">Tag</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        IEnumerable<TaggedEntity> GetTaggedMembersByTag(string tag, string tagGroup = null);

        /// <summary>
        /// Gets every tag stored in the database
        /// </summary>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        IEnumerable<ITag> GetAllTags(string tagGroup = null);

        /// <summary>
        /// Gets all tags for content items
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        IEnumerable<ITag> GetAllContentTags(string tagGroup = null);

        /// <summary>
        /// Gets all tags for media items
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        IEnumerable<ITag> GetAllMediaTags(string tagGroup = null);

        /// <summary>
        /// Gets all tags for member items
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        IEnumerable<ITag> GetAllMemberTags(string tagGroup = null);

        /// <summary>
        /// Gets all tags attached to a property by entity id
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="propertyTypeAlias">Property type alias</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string tagGroup = null);

        /// <summary>
        /// Gets all tags attached to an entity (content, media or member) by entity id
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        IEnumerable<ITag> GetTagsForEntity(int contentId, string tagGroup = null);

        /// <summary>
        /// Gets all tags attached to a property by entity id
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="propertyTypeAlias">Property type alias</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        IEnumerable<ITag> GetTagsForProperty(Guid contentId, string propertyTypeAlias, string tagGroup = null);

        /// <summary>
        /// Gets all tags attached to an entity (content, media or member) by entity id
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        IEnumerable<ITag> GetTagsForEntity(Guid contentId, string tagGroup = null);
    }
}