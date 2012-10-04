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
    /// <typeparam name="TEntity"></typeparam>
    internal abstract class Repository<TEntity> : IDisposable, 
        IRepository<TEntity> where TEntity : class, IAggregateRoot
    {
        private IUnitOfWork<Database> _work;
        private readonly IRepositoryCacheProvider _cache;

        protected Repository(IUnitOfWork<Database> work)
            : this(work, RuntimeCacheProvider.Current)
        {
        }

        internal Repository(IUnitOfWork<Database> work, IRepositoryCacheProvider cache)
        {
            _work = work;
            _cache = cache;
        }

        internal IUnitOfWork<Database> UnitOfWork
        {
            get { return _work; }
        }

        protected abstract void PerformAdd(TEntity entity);
        protected abstract void PerformUpdate(TEntity entity);
        public void AddOrUpdate(TEntity entity)
        {
            if (!entity.HasIdentity)
            {
                PerformAdd(entity);
            }
            else
            {
                PerformUpdate(entity);
            }

            _cache.Save(typeof(TEntity), entity);
        }

        protected abstract void PerformDelete(TEntity entity);
        public void Delete(TEntity entity)
        {
            _cache.Delete(typeof(TEntity), entity);
            PerformDelete(entity);
        }

        protected abstract TEntity PerformGet(int id);
        public TEntity Get(int id)
        {
            var rEntity = _cache.GetById(typeof(TEntity), ConvertIdToGuid(id));
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

        protected abstract IEnumerable<TEntity> PerformGetAll(params int[] ids);
        public IEnumerable<TEntity> GetAll(params int[] ids)
        {
            if (ids.Any())
            {
                var entities = _cache.GetByIds(typeof(TEntity), ids.Select(ConvertIdToGuid).ToList());
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
        public IEnumerable<TEntity> GetByQuery(IQuery<TEntity> query)
        {
            return PerformGetByQuery(query);
        }

        protected abstract bool PerformExists(int id);
        public bool Exists(int id)
        {
            var rEntity = _cache.GetById(typeof(TEntity), ConvertIdToGuid(id));
            if (rEntity != null)
            {
                return true;
            }

            return PerformExists(id);
        }

        protected abstract int PerformCount(IQuery<TEntity> query);
        public int Count(IQuery<TEntity> query)
        {
            return PerformCount(query);
        }

        public void SetUnitOfWork<T>(IUnitOfWork<T> work)
        {
            _work = work as IUnitOfWork<Database>;
        }

        public virtual void Dispose()
        {
            _work.Dispose();
        }

        /// <summary>
        /// Internal method that handles the convertion of an object Id
        /// to an Integer and then a Guid Id.
        /// </summary>
        /// <remarks>In the future it should be possible to change this method
        /// so it converts from object to guid if/when we decide to go from
        /// int to guid based ids.</remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual Guid ConvertIdToGuid(int id)
        {
            return id.ToGuid();
        }
    }
}