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
    internal abstract class RepositoryBase<TId, TEntity> : IRepositoryQueryable<TId, TEntity>, 
        IUnitOfWorkRepository where TEntity : IAggregateRoot
    {
        private IUnitOfWork _work;
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
        protected IUnitOfWork UnitOfWork
        {
            get { return _work; }
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

            _cache.Save(typeof(TEntity), entity);
        }

        /// <summary>
        /// Deletes the passed in entity
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(TEntity entity)
        {
            _cache.Delete(typeof(TEntity), entity);
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
            Guid key = id is int ? ConvertIdToGuid(id) : ConvertStringIdToGuid(id.ToString());
            var rEntity = _cache.GetById(typeof(TEntity), key);
            if (rEntity != null)
            {
                return (TEntity)rEntity;
            }

            var entity = PerformGet(id);
            if (entity != null)
            {
                _cache.Save(typeof(TEntity), entity);
            }

            return entity;
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
                if (ids.Count().Equals(entities.Count()))
                    return entities.Select(x => (TEntity)x);
            }
            else
            {
                var allEntities = _cache.GetAllByType(typeof(TEntity));
                if (allEntities.Any())
                    return allEntities.Select(x => (TEntity)x);
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
            Guid key = id is int ? ConvertIdToGuid(id) : ConvertStringIdToGuid(id.ToString());
            var rEntity = _cache.GetById(typeof(TEntity), key);
            if (rEntity != null)
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

        /// <summary>
        /// Sets the repository's Unit Of Work with the passed in <see cref="IUnitOfWork"/>
        /// </summary>
        /// <param name="work"></param>
        public void SetUnitOfWork(IUnitOfWork work)
        {
            _work = work;
        }

        #endregion

        #region IUnitOfWorkRepository Members

        /// <summary>
        /// Unit of work method that tells the repository to persist the new entity
        /// </summary>
        /// <param name="item"></param>
        public virtual void PersistNewItem(IEntity item)
        {
            PersistNewItem((TEntity)item);
        }

        /// <summary>
        /// Unit of work method that tells the repository to persist the updated entity
        /// </summary>
        /// <param name="item"></param>
        public virtual void PersistUpdatedItem(IEntity item)
        {
            PersistUpdatedItem((TEntity)item);
        }

        /// <summary>
        /// Unit of work method that tells the repository to persist the deletion of the entity
        /// </summary>
        /// <param name="item"></param>
        public virtual void PersistDeletedItem(IEntity item)
        {
            PersistDeletedItem((TEntity)item);
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
    }
}