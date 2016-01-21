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
    }
}