using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.Caching
{
    /// <summary>
    /// The InMemory registry looks up objects in an in-memory dictionary for fast retrival
    /// </summary>
    internal class InMemoryCacheProvider : IRepositoryCacheProvider
    {
        #region Singleton

        private static readonly Lazy<InMemoryCacheProvider> lazy = new Lazy<InMemoryCacheProvider>(() => new InMemoryCacheProvider());

        public static InMemoryCacheProvider Current { get { return lazy.Value; } }

        private InMemoryCacheProvider()
        {
        }

        #endregion

        private readonly ConcurrentDictionary<string, IEntity> _cache = new ConcurrentDictionary<string, IEntity>();

        /// <summary>
        /// Retrives an object of the specified type by its Id
        /// </summary>
        /// <param name="type">The type of the object to retrive, which implements <see cref="IEntity"/></param>
        /// <param name="id">The Guid Id of the Object to retrive</param>
        /// <returns></returns>
        public IEntity GetById(Type type, Guid id)
        {
            var compositeKey = GetCompositeId(type, id);
            var containsKey = _cache.ContainsKey(compositeKey);
            if (containsKey)
            {
                var result = _cache[compositeKey];

                //IMPORTANT: we must clone to resolve, see: http://issues.umbraco.org/issue/U4-4259
                return (IEntity)result.DeepClone();
            }

            return null;
        }

        /// <summary>
        /// Retrives objects of the specified type by their Ids
        /// </summary>
        /// <param name="type">The type of the objects to retrive, which implements <see cref="IEntity"/></param>
        /// <param name="ids">The Guid Ids of the Objects to retrive</param>
        /// <returns></returns>
        public IEnumerable<IEntity> GetByIds(Type type, List<Guid> ids)
        {
            var list = (from id in ids
                        select GetCompositeId(type, id)
                            into key
                            let containsKey = _cache.ContainsKey(key)
                            where containsKey
                            select _cache[key]
                            into result 
                            //don't return null objects
                            where result != null
                            //IMPORTANT: we must clone to resolve, see: http://issues.umbraco.org/issue/U4-4259
                            select (IEntity)result.DeepClone()).ToList();
            return list;
        }

        /// <summary>
        /// Retrives all objects of the specified type
        /// </summary>
        /// <param name="type">The type of the objects to retrive, which implements <see cref="IEntity"/></param>
        /// <returns></returns>
        public IEnumerable<IEntity> GetAllByType(Type type)
        {
            var list = _cache.Keys
                .Where(key => key.Contains(type.Name))
                .Select(key => _cache[key])
                //don't return null objects
                .Where(result => result != null)
                //IMPORTANT: we must clone to resolve, see: http://issues.umbraco.org/issue/U4-4259
                .Select(result => (IEntity)result.DeepClone())
                .ToList();
            return list;
        }

        /// <summary>
        /// Saves an object in the registry cache
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="entity"></param>
        public void Save(Type type, IEntity entity)
        {
            //IMPORTANT: we must clone to store, see: http://issues.umbraco.org/issue/U4-4259
            entity = (IEntity)entity.DeepClone();

            _cache.AddOrUpdate(GetCompositeId(type, entity.Id), entity, (x, y) => entity);
        }

        /// <summary>
        /// Deletes an object from the registry cache
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="entity"></param>
        public void Delete(Type type, IEntity entity)
        {
            IEntity entity1;
            bool result = _cache.TryRemove(GetCompositeId(type, entity.Id), out entity1);
        }

        /// <summary>
        /// Clear cache by type
        /// </summary>
        /// <param name="type"></param>
        public void Clear(Type type)
        {
            var keys = _cache.Keys;
            foreach (var key in keys.Where(x => x.StartsWith(string.Format("{0}-", type.Name))))
            {
                IEntity e;
                _cache.TryRemove(key, out e);
            }
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private string GetCompositeId(Type type, Guid id)
        {
            return string.Format("{0}-{1}", type.Name, id.ToString());
        }

        private string GetCompositeId(Type type, int id)
        {
            return string.Format("{0}-{1}", type.Name, id.ToGuid());
        }
    }
}