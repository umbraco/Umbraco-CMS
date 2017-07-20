using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Defines the base implementation of a repository.
    /// </summary>
    public interface IRepository
    { }

    public interface IReadRepository<in TId, out TEntity> : IRepository
    {
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
        /// Boolean indicating whether an Entity with the specified Id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exists(TId id);
    }

    public interface IWriteRepository<in TEntity> : IRepository
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
    }

    public interface IRepository<in TId, TEntity> : IReadRepository<TId, TEntity>, IWriteRepository<TEntity>
    { }
}
