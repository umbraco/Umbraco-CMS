using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ISchemaTypeRepository : IContentTypeCompositionRepository<ISchemaType>
    {
        /// <summary>
        /// Gets all entities of the specified <see cref="PropertyType"/> query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>An enumerable list of <see cref="IContentType"/> objects</returns>
        IEnumerable<ISchemaType> GetByQuery(IQuery<PropertyType> query);

        IEnumerable<MoveEventInfo<ISchemaType>> Move(ISchemaType toMove, EntityContainer container);

        /// <summary>
        /// Derives a unique alias from an existing alias.
        /// </summary>
        /// <param name="alias">The original alias.</param>
        /// <returns>The original alias with a number appended to it, so that it is unique.</returns>
        /// <remarks>Unique accross all content, media and member types.</remarks>
        string GetUniqueAlias(string alias);
    }
}