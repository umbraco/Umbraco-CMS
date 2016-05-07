using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IContentTypeRepositoryBase<TItem> : IRepositoryQueryable<int, TItem>, IReadRepository<Guid, TItem>
        where TItem : IContentTypeComposition
    {
        TItem Get(string alias);
        IEnumerable<MoveEventInfo<TItem>> Move(TItem moving, EntityContainer container);
        IEnumerable<TItem> GetTypesDirectlyComposedOf(int id);

        /// <summary>
        /// Derives a unique alias from an existing alias.
        /// </summary>
        /// <param name="alias">The original alias.</param>
        /// <returns>The original alias with a number appended to it, so that it is unique.</returns>
        /// <remarks>Unique accross all content, media and member types.</remarks>
        string GetUniqueAlias(string alias);
    }
}