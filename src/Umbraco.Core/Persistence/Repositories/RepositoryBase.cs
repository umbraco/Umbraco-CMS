using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represent an abstract Repository, which is the base of the Repository implementations
    /// </summary>
    /// <typeparam name="TEntity">Type of <see cref="IAggregateRoot"/> entity for which the repository is used</typeparam>
    /// <typeparam name="TId">Type of the Id used for this entity</typeparam>
    internal abstract class RepositoryBase<TId, TEntity> : DisposableObject, IRepositoryQueryable<TId, TEntity>, IUnitOfWorkRepository 
		where TEntity : class, IAggregateRoot
    {
		private readonly IUnitOfWork _work;
        private readonly IRepositoryCacheProvider _cache;

		protected RepositoryBase(IUnitOfWork work)
            : this(work, RuntimeCacheProvider.Current)
        {
        }

		internal RepositoryBase(IUnitOfWork work, IRepositoryCacheProvider cache)
        {
            _work = work;
            _cache = cache;
        }

        /// <summary>
        /// Returns the Unit of Work added to the repository
        /// </summary>
		protected internal IUnitOfWork UnitOfWork
        {
            get { return _work; }
        }

        /// <summary>
        /// Internal for testing purposes
        /// </summary>
        internal Guid UnitKey
        {
            get { return (Guid)_work.Key; }
        }

        #region IRepository<TEntity> Members

        /// <summary>
        /// Adds or Updates an entity of type TEntity
        /// </summary>
        /// <remarks>This method is backed by an <see cref="IRepositoryCacheProvider"/> cache</remarks>
        /// <param name="entity"></param>
        public void AddOrUpdate(TEntity entity)
        {
            if (!entity.HasIdentity)
            {
                _work.RegisterAdded(entity, this);
            }
            else
            {
                _work.RegisterChanged(entity, this);
            }
        }

        /// <summary>
        /// Deletes the passed in entity
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(TEntity entity)
        {
            if(_work != null)
            {
                _work.RegisterRemoved(entity, this);
            }
        }

        protected abstract TEntity PerformGet(TId id);
        /// <summary>
        /// Gets an entity by the passed in Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Get(TId id)
        {
            var fromCache = TryGetFromCache(id);
            if (fromCache.Success)
            {
                return fromCache.Result;
            }

            var entity = PerformGet(id);
            if (entity != null)
            {
                _cache.Save(typeof(TEntity), entity);
            }

            if (entity != null)
            {
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                TracksChangesEntityBase asEntity = entity as TracksChangesEntityBase;
                if (asEntity != null)
                {
                    asEntity.ResetDirtyProperties(false);
                }
            }
            
            return entity;
        }

        protected Attempt<TEntity> TryGetFromCache(TId id)
        {
            Guid key = id is int ? ConvertIdToGuid(id) : ConvertStringIdToGuid(id.ToString());
            var rEntity = _cache.GetById(typeof(TEntity), key);
            if (rEntity != null)
            {
                return new Attempt<TEntity>(true, (TEntity) rEntity);
            }
            return Attempt<TEntity>.False;
        } 

        protected abstract IEnumerable<TEntity> PerformGetAll(params TId[] ids);
        /// <summary>
        /// Gets all entities of type TEntity or a list according to the passed in Ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetAll(params TId[] ids)
        {
            if (ids.Any())
            {
                var entities = _cache.GetByIds(typeof(TEntity), ids.Select(id => id is int ? ConvertIdToGuid(id) : ConvertStringIdToGuid(id.ToString())).ToList());
                if (ids.Count().Equals(entities.Count()) && entities.Any(x => x == null) == false)
                    return entities.Select(x => (TEntity)x);
            }
            else
            {
                var allEntities = _cache.GetAllByType(typeof(TEntity));
                
                if (allEntities.Any())
                {
                    //Get count of all entities of current type (TEntity) to ensure cached result is correct
                    var query = Query<TEntity>.Builder.Where(x => x.Id != 0);
                    int totalCount = PerformCount(query);

                    if(allEntities.Count() == totalCount)
                        return allEntities.Select(x => (TEntity)x);
                }
            }

            var entityCollection = PerformGetAll(ids);

            foreach (var entity in entityCollection)
            {
                if (entity != null)
                {
                    _cache.Save(typeof(TEntity), entity);
                }
            }

            return entityCollection;
        }

        protected abstract IEnumerable<TEntity> PerformGetByQuery(IQuery<TEntity> query);
        /// <summary>
        /// Gets a list of entities by the passed in query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetByQuery(IQuery<TEntity> query)
        {
            return PerformGetByQuery(query);
        }

        protected abstract bool PerformExists(TId id);
        /// <summary>
        /// Returns a boolean indicating whether an entity with the passed Id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Exists(TId id)
        {
            var fromCache = TryGetFromCache(id);
            if (fromCache.Success)
            {
                return true;
            }
            return PerformExists(id);            
        }

        protected abstract int PerformCount(IQuery<TEntity> query);
        /// <summary>
        /// Returns an integer with the count of entities found with the passed in query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int Count(IQuery<TEntity> query)
        {
            return PerformCount(query);
        }       

        #endregion

        #region IUnitOfWorkRepository Members

        /// <summary>
        /// Unit of work method that tells the repository to persist the new entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistNewItem(IEntity entity)
        {
            PersistNewItem((TEntity)entity);
            _cache.Save(typeof(TEntity), entity);
        }

        /// <summary>
        /// Unit of work method that tells the repository to persist the updated entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistUpdatedItem(IEntity entity)
        {
            PersistUpdatedItem((TEntity)entity);
            _cache.Save(typeof(TEntity), entity);
        }

        /// <summary>
        /// Unit of work method that tells the repository to persist the deletion of the entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistDeletedItem(IEntity entity)
        {
            PersistDeletedItem((TEntity)entity);
            _cache.Delete(typeof(TEntity), entity);
        }

        #endregion

        #region Abstract IUnitOfWorkRepository Methods

        protected abstract void PersistNewItem(TEntity item);
        protected abstract void PersistUpdatedItem(TEntity item);
        protected abstract void PersistDeletedItem(TEntity item);

        #endregion

        /// <summary>
        /// Internal method that handles the convertion of an object Id
        /// to an Integer and then a Guid Id.
        /// </summary>
        /// <remarks>In the future it should be possible to change this method
        /// so it converts from object to guid if/when we decide to go from
        /// int to guid based ids.</remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual Guid ConvertIdToGuid(TId id)
        {
            int i = 0;
            if(int.TryParse(id.ToString(), out i))
            {
                return i.ToGuid();
            }
            return ConvertStringIdToGuid(id.ToString());
        }

        protected virtual Guid ConvertStringIdToGuid(string id)
        {
            return id.EncodeAsGuid();
        }

		/// <summary>
		/// Dispose disposable properties
		/// </summary>
		/// <remarks>
		/// Ensure the unit of work is disposed
		/// </remarks>
		protected override void DisposeResources()
		{
			UnitOfWork.DisposeIfDisposable();
		}
    }
}