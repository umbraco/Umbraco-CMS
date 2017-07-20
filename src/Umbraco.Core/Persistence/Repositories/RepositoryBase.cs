using System;
using System.Collections.Generic;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Provides a base class to all repositories.
    /// </summary>
    internal abstract class RepositoryBase
    {
        private static readonly Dictionary<Type, string> CacheTypeKeys = new Dictionary<Type, string>();

        protected RepositoryBase(IScopeUnitOfWork work, CacheHelper cache, ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            UnitOfWork = work ?? throw new ArgumentNullException(nameof(work));
            GlobalCache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        protected IScopeUnitOfWork UnitOfWork { get; }

        protected CacheHelper GlobalCache { get; }

        protected abstract IRuntimeCacheProvider IsolatedCache { get; }

        protected ILogger Logger { get; }

        public static string GetCacheIdKey<T>(object id) => GetCacheTypeKey<T>() + id;

        public static string GetCacheTypeKey<T>()
        {
            var type = typeof(T);
            return CacheTypeKeys.TryGetValue(type, out string key) ? key : (CacheTypeKeys[type] = "uRepo_" + type.Name + "_");
        }
    }
}
