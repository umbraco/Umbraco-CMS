using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ITagRepository : IReadWriteQueryRepository<int, ITag>
    {
        #region Assign and Remove Tags

        /// <summary>
        /// Assign tags to a content property.
        /// </summary>
        /// <param name="contentId">The identifier of the content item.</param>
        /// <param name="propertyTypeId">The identifier of the property type.</param>
        /// <param name="tags">The tags to assign.</param>
        /// <param name="replaceTags">A value indicating whether to replace already assigned tags.</param>
        /// <remarks>
        /// <para>When <paramref name="replaceTags"/> is false, the tags specified in <paramref name="tags"/> are added to those already assigned.</para>
        /// <para>When <paramref name="tags"/> is empty and <paramref name="replaceTags"/> is true, all assigned tags are removed.</para>
        /// </remarks>
        void Assign(int contentId, int propertyTypeId, IEnumerable<ITag> tags, bool replaceTags);

        /// <summary>
        /// Removes assigned tags from a content property.
        /// </summary>
        /// <param name="contentId">The identifier of the content item.</param>
        /// <param name="propertyTypeId">The identifier of the property type.</param>
        /// <param name="tags">The tags to remove.</param>
        void Remove(int contentId, int propertyTypeId, IEnumerable<ITag> tags);

        /// <summary>
        /// Removes all assigned tags from a content item.
        /// </summary>
        /// <param name="contentId">The identifier of the content item.</param>
        void RemoveAll(int contentId);
        
        /// <summary>
        /// Removes all assigned tags from a content property.
        /// </summary>
        /// <param name="contentId">The identifier of the content item.</param>
        /// <param name="propertyTypeId">The identifier of the property type.</param>
        void RemoveAll(int contentId, int propertyTypeId);

        #endregion

        #region Queries

        TaggedEntity GetTaggedEntityByKey(Guid key);
        TaggedEntity GetTaggedEntityById(int id);

        IEnumerable<TaggedEntity> GetTaggedEntitiesByTagGroup(TaggableObjectTypes objectType, string tagGroup);

        IEnumerable<TaggedEntity> GetTaggedEntitiesByTag(TaggableObjectTypes objectType, string tag, string tagGroup = null);

        /// <summary>
        /// Returns all tags for an entity type (content/media/member)
        /// </summary>
        /// <param name="objectType">Entity type</param>
        /// <param name="group">Optional group</param>
        /// <returns></returns>
        IEnumerable<ITag> GetTagsForEntityType(TaggableObjectTypes objectType, string group = null);

        /// <summary>
        /// Returns all tags that exist on the content item - Content/Media/Member
        /// </summary>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="group">Optional group</param>
        /// <returns></returns>
        IEnumerable<ITag> GetTagsForEntity(int contentId, string group = null);

        /// <summary>
        /// Returns all tags that exist on the content item - Content/Media/Member
        /// </summary>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="group">Optional group</param>
        /// <returns></returns>
        IEnumerable<ITag> GetTagsForEntity(Guid contentId, string group = null);

        /// <summary>
        /// Returns all tags that exist on the content item for the property specified - Content/Media/Member
        /// </summary>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="propertyTypeAlias">The property alias to get tags for</param>
        /// <param name="group">Optional group</param>
        /// <returns></returns>
        IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string group = null);

        /// <summary>
        /// Returns all tags that exist on the content item for the property specified - Content/Media/Member
        /// </summary>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="propertyTypeAlias">The property alias to get tags for</param>
        /// <param name="group">Optional group</param>
        /// <returns></returns>
        IEnumerable<ITag> GetTagsForProperty(Guid contentId, string propertyTypeAlias, string group = null);

        #endregion
    }
}
