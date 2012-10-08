using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Defines the implementation of a Repository
    /// </summary>
    /// <typeparam name="TEntity">Type of <see cref="IAggregateRoot"/> entity for which the repository is used</typeparam>
    /// <typeparam name="TId">Type of the Id used for this entity</typeparam>
    public interface IRepository<TId, TEntity> 
        where TEntity : IAggregateRoot
    {
        /// <summary>
        /// Adds or Updates an Entity
        /// </summary>
        /// <param name="entity"></param>
        void AddOrUpdate(TEntity entity);

        /// <summary>
        /// Deletes an Entity
        /// </summary>
        /// <param name="entity"></param>
        void Delete(TEntity entity);

        /// <summary>
        /// Gets an Entity by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity Get(TId id);

        /// <summary>
        /// Gets all entities of the spefified type
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        IEnumerable<TEntity> GetAll(params TId[] ids);

        /// <summary>
        /// Gets all entities of the spefified type and query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<TEntity> GetByQuery(IQuery<TEntity> query);

        /// <summary>
        /// Boolean indicating whether an Entity with the specified Id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exists(TId id);

        /// <summary>
        /// Returns the count for the specified query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int Count(IQuery<TEntity> query);

        /// <summary>
        /// Sets the Unit Of Work for the Repository
        /// </summary>
        /// <param name="work"></param>
        void SetUnitOfWork(IUnitOfWork work);
    }
}