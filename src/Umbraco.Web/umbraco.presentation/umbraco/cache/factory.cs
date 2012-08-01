using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Resolving;
using Umbraco.Web;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;
using System.Collections.Generic;

namespace umbraco.presentation.cache
{
    /// <summary>
    /// cache.factory uses reflection to find all registered instances of ICacheRefresher.
    /// </summary>
	[Obsolete("Use Umbraco.Core.CacheRefreshersResolver instead")]
    public class Factory
    {	

        #region Methods
		
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
			return CacheRefreshersResolver.Current.GetById(CacheRefresherId);
        }

    	/// <summary>
        /// Gets all ICacheRefreshers
        /// </summary>
        /// <returns></returns>
        public ICacheRefresher[] GetAll()
    	{
    		return CacheRefreshersResolver.Current.CacheResolvers.ToArray();    		
    	}

        #endregion
    }
}