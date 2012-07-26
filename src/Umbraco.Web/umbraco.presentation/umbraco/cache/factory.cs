using System;
using System.Web;

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

        private static readonly Dictionary<Guid, Type> _refreshers = new Dictionary<Guid, Type>();

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

        private static void Initialize()
        {
        	var typeFinder = new Umbraco.Core.TypeFinder2();
			var types = typeFinder.FindClassesOfType<ICacheRefresher>();
            foreach (var t in types)
            {
                var typeInstance = Activator.CreateInstance(t) as ICacheRefresher;
                if (typeInstance != null)
                    _refreshers.Add(typeInstance.UniqueIdentifier, t);
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
            ICacheRefresher newObject = Activator.CreateInstance(_refreshers[CacheRefresherId]) as ICacheRefresher;
            return newObject;
        }

        /// <summary>
        /// Gets all ICacheRefreshers
        /// </summary>
        /// <returns></returns>
        public ICacheRefresher[] GetAll()
        {
            ICacheRefresher[] retVal = new ICacheRefresher[_refreshers.Count];
            int c = 0;

            foreach (ICacheRefresher cr in _refreshers.Values)
            {
                retVal[c] = GetNewObject(cr.UniqueIdentifier);
                c++;
            }

            return retVal;
        }

        #endregion
    }
}