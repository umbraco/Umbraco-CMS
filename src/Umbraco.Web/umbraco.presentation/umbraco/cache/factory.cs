using System;
using System.Collections.Concurrent;
using System.Web;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;
using System.Collections.Generic;

namespace umbraco.presentation.cache
{
    /// <summary>
    /// cache.factory uses reflection to find all registered instances of ICacheRefresher.
    /// </summary>
    public class Factory
    {
        #region Declarations

		internal static readonly ConcurrentDictionary<Guid, Type> _refreshers = new ConcurrentDictionary<Guid, Type>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="Factory"/> class.
        /// </summary>
        static Factory()
        {
            Initialize();
        }

        #endregion

        #region Methods

        internal static void Initialize()
        {
			foreach(var t in PluginTypeResolver.Current.ResolveCacheRefreshers())
			{
				var instance = PluginTypeResolver.Current.CreateInstance<ICacheRefresher>(t);
				if (instance != null)
				{
					_refreshers.TryAdd(instance.UniqueIdentifier, t);	
				}
			}        	
        }

        public ICacheRefresher CacheRefresher(Guid CacheRefresherId)
        {
            return GetNewObject(CacheRefresherId);
        }

        /// <summary>
        /// Gets the IcacheRefresher object with the specified Guid.
        /// </summary>
        /// <param name="CacheRefresherId">The cache refresher guid.</param>
        /// <returns></returns>
        public ICacheRefresher GetNewObject(Guid CacheRefresherId)
        {
        	return !_refreshers.ContainsKey(CacheRefresherId) 
				? null 
				: PluginTypeResolver.Current.CreateInstance<ICacheRefresher>(_refreshers[CacheRefresherId]);
        }

    	/// <summary>
        /// Gets all ICacheRefreshers
        /// </summary>
        /// <returns></returns>
        public ICacheRefresher[] GetAll()
        {
			var retVal = new ICacheRefresher[_refreshers.Count];
			var c = 0;

			foreach (var id in _refreshers.Keys)
			{
				retVal[c] = GetNewObject(id);
				c++;
			}

			return retVal;
        }

        #endregion
    }
}