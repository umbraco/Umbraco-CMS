using System.Collections.Generic;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
	public interface IRepository
	{
		/// <summary>
		/// Sets the Unit Of Work for the Repository
		/// </summary>
		/// <param name="work"></param>
		void SetUnitOfWork(IUnitOfWork work);
	}

	/// <summary>
    /// Defines the implementation of a Repository
    /// </summary>
    public interface IRepository<in TId, TEntity> : IRepository
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
        /// Boolean indicating whether an Entity with the specified Id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exists(TId id);
	}
}