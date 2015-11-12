using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IContentTypeRepository : IRepositoryQueryable<int, IContentType>, IReadRepository<Guid, IContentType>
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

        /// <summary>
        /// Creates a folder for content types
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        EntityContainer CreateContainer(int parentId, string name, int userId);

        /// <summary>
        /// Deletes a folder - this will move all contained content types into their parent
        /// </summary>
        /// <param name="containerId"></param>
        void DeleteContainer(int containerId);
    }
}