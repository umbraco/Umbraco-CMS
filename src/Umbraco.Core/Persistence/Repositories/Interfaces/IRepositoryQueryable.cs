using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    //TODO: This should be decoupled! Shouldn't inherit from the IRepository

    /// <summary>
    /// Defines the implementation of a Repository, which allows queries against the <see cref="TEntity"/>
    /// </summary>
    /// <typeparam name="TEntity">Type of <see cref="IAggregateRoot"/> entity for which the repository is used</typeparam>
    /// <typeparam name="TId">Type of the Id used for this entity</typeparam>
    public interface IRepositoryQueryable<in TId, TEntity> : IRepository<TId, TEntity>
    {
        /// <summary>
        /// Gets all entities of the specified type and query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<TEntity> GetByQuery(IQuery<TEntity> query);

        /// <summary>
        /// Returns the count for the specified query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int Count(IQuery<TEntity> query);
    }
}