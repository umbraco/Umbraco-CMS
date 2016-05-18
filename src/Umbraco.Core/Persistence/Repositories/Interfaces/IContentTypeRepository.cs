using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IContentTypeRepository : IContentTypeCompositionRepository<IContentType>
    {
        /// <summary>
        /// Gets all entities of the specified <see cref="PropertyType"/> query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>An enumerable list of <see cref="IContentType"/> objects</returns>
        IEnumerable<IContentType> GetByQuery(IQuery<PropertyType> query);

        /// <summary>
        /// Gets all property type aliases.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllPropertyTypeAliases();

        IEnumerable<MoveEventInfo<IContentType>> Move(IContentType toMove, EntityContainer container);

        /// <summary>
        /// Gets all content type aliases
        /// </summary>
        /// <param name="objectTypes">
        /// If this list is empty, it will return all content type aliases for media, members and content, otherwise
        /// it will only return content type aliases for the object types specified
        /// </param>
        /// <returns></returns>
        IEnumerable<string> GetAllContentTypeAliases(params Guid[] objectTypes);

        /// <summary>
        /// Derives a unique alias from an existing alias.
        /// </summary>
        /// <param name="alias">The original alias.</param>
        /// <returns>The original alias with a number appended to it, so that it is unique.</returns>
        /// /// <remarks>Unique accross all content, media and member types.</remarks>
        string GetUniqueAlias(string alias);

        /// <summary>
        /// Moves a property group to a new content type
        /// </summary>
        /// <param name="propertyGroupId">The property group Id</param>
        /// <param name="sortOrder">The new sort order for the group</param>
        /// <param name="contentTypeId">The Id of the content type to move to</param>
        /// <returns>Id of newly created property group</returns>
        int CopyPropertyGroup(int propertyGroupId, int sortOrder, int contentTypeId);

        /// <summary>
        /// Moves a property to a new content type
        /// </summary>
        /// <param name="propertyId">The property Id</param>
        /// <param name="propertyGroupId">The Id of the property group to move to</param>
        /// <param name="contentTypeId">The Id of the content type to move to</param>
        void MovePropertyType(int propertyId ,int propertyGroupId, int contentTypeId);

        /// <summary>
        /// Creates a composition relation between two document types
        /// </summary>
        /// <param name="parentId">Parent type</param>
        /// <param name="childId">Child type</param>
        void CreateCompositionRelation(int parentId, int childId);
    }
}