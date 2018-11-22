using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    // Note: RoutesCache closely follows the caching strategy dating from v4, which
    // is obviously broken in many ways (eg it's a global cache but relying to some
    // extend to the content cache, which itself is local to each request...).
    // Not going to fix it anyway.

    class RoutesCache
    {
        private ConcurrentDictionary<int, string> _routes;
        private ConcurrentDictionary<string, int> _nodeIds;

        // NOTE
        // RoutesCache is cleared by
        // - ContentTypeCacheRefresher, whenever anything happens to any content type
        // - DomainCacheRefresher, whenever anything happens to any domain
        // - XmlStore, whenever anything happens to the XML cache

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutesCache"/> class.
        /// </summary>
        public RoutesCache()
        {
            Clear();
        }

        /// <summary>
        /// Used ONLY for unit tests
        /// </summary>
        /// <returns></returns>
        internal IDictionary<int, string> GetCachedRoutes()
        {
            return _routes;
        }

        /// <summary>
        /// Used ONLY for unit tests
        /// </summary>
        /// <returns></returns>
        internal IDictionary<string, int> GetCachedIds()
        {
            return _nodeIds;
        }

        #region Public

        /// <summary>
        /// Stores a route for a node.
        /// </summary>
        /// <param name="nodeId">The node identified.</param>
        /// <param name="route">The route.</param>
        /// <param name="trust">A value indicating whether the value can be trusted for inbound routing.</param>
        public void Store(int nodeId, string route, bool trust)
        {
            _routes.AddOrUpdate(nodeId, i => route, (i, s) => route);
            if (trust)
                _nodeIds.AddOrUpdate(route, i => nodeId, (i, s) => nodeId);
        }

        /// <summary>
        /// Gets a route for a node.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <returns>The route for the node, else null.</returns>
        public string GetRoute(int nodeId)
        {
            string val;
            _routes.TryGetValue(nodeId, out val);
            return val;
        }

        /// <summary>
        /// Gets a node for a route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns>The node identified for the route, else zero.</returns>
        public int GetNodeId(string route)
        {
            int val;
            _nodeIds.TryGetValue(route, out val);
            return val;
        }

        /// <summary>
        /// Clears the route for a node.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        public void ClearNode(int nodeId)
        {
            string route;
            if (_routes.TryRemove(nodeId, out route))
            {
                int id;
                _nodeIds.TryRemove(route, out id);
            }
        }

        /// <summary>
        /// Clears all routes.
        /// </summary>
        public void Clear()
        {
            _routes = new ConcurrentDictionary<int, string>();
            _nodeIds = new ConcurrentDictionary<string, int>();
        }

        #endregion
    }
}
