using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.Caching
{
    internal class NullCacheProvider : IRepositoryCacheProvider
    {
        #region Singleton
        private static volatile NullCacheProvider _instance;
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        private NullCacheProvider() { }

        public static NullCacheProvider Current
        {
            get
            {
                using (new WriteLock(Lock))
                {
                    if (_instance == null) _instance = new NullCacheProvider();
                }

                return _instance;
            }
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

        #endregion
    }
}