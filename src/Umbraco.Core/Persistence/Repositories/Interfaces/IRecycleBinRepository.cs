using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IRecycleBinRepository<out TEntity>
        where TEntity : IAggregateRoot
    {
        /// <summary>
        /// Gets all entities in recycle bin
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> GetEntitiesInRecycleBin();
        
        /// <summary>
        /// Called to empty the recycle bin
        /// </summary>
        /// <returns></returns>
        bool EmptyRecycleBin();
       
    }
}