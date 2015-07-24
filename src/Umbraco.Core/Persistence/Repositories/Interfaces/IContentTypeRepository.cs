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
    }
}