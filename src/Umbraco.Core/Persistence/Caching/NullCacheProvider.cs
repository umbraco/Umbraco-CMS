using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.Caching
{
    internal class NullCacheProvider : IRepositoryCacheProvider
    {
        #region Singleton

        private static readonly Lazy<NullCacheProvider> lazy = new Lazy<NullCacheProvider>(() => new NullCacheProvider());

        public static NullCacheProvider Current { get { return lazy.Value; } }

        private NullCacheProvider()
        {
        }

        #endregion

        #region Implementation of IRepositoryCacheProvider

        public IEntity GetById(Type type, Guid id)
        {
            return null;
        }

        public IEnumerable<IEntity> GetByIds(Type type, List<Guid> ids)
        {
            return Enumerable.Empty<IEntity>();
        }

        public IEnumerable<IEntity> GetAllByType(Type type)
        {
            return Enumerable.Empty<IEntity>();
        }

        public void Save(Type type, IEntity entity)
        {
            return;
        }

        public void Delete(Type type, IEntity entity)
        {
            return;
        }

        public void Clear(Type type)
        {
            return;
        }

        #endregion
    }
}