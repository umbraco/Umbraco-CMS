using System.Collections.Generic;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    //TODO: This should be decoupled! Shouldn't inherit from the IRepository

    public interface IQueryRepository<in TId, TEntity> : IRepository<TId, TEntity>
    {
        /// <summary>
        /// Creates a new query.
        /// </summary>
        IQuery<TEntity> QueryT { get; }

        /// <summary>
        /// Creates a new query.
        /// </summary>
        IQuery<T> Query<T>();

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