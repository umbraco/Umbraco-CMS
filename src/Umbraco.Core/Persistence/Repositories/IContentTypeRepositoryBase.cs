using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IContentTypeRepositoryBase<TItem> : IReadWriteQueryRepository<int, TItem>, IReadRepository<Guid, TItem>
        where TItem : IContentTypeComposition
    {
        TItem Get(string alias);
        IEnumerable<MoveEventInfo<TItem>> Move(TItem moving, EntityContainer container);

        /// <summary>
        /// Derives a unique alias from an existing alias.
        /// </summary>
        /// <param name="alias">The original alias.</param>
        /// <returns>The original alias with a number appended to it, so that it is unique.</returns>
        /// <remarks>Unique across all content, media and member types.</remarks>
        string GetUniqueAlias(string alias);


        /// <summary>
        /// Gets a value indicating whether there is a list view content item in the path.
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        bool HasContainerInPath(string contentPath);
    }
}
