using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides a default implementation of <see cref="IRoutesCache"/>.
	/// </summary>
	internal class DefaultRoutesCache : IRoutesCache
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		private Dictionary<int, string> _routes;
        private Dictionary<string, int> _nodeIds;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultRoutesCache"/> class.
		/// </summary>
        public DefaultRoutesCache()
        {
            Clear();

			//FIXME:
			//
            // here we must register handlers to clear the cache when content changes
			// this was done by presentation.library, which cleared everything when content changed
            // but really, we should do some partial refreshes

			// these are the two events that were used by presentation.library
			// are they enough?

			global::umbraco.content.AfterRefreshContent += (sender, e) => Clear();
			global::umbraco.content.AfterUpdateDocumentCache += (sender, e) => Clear();
        }

		/// <summary>
		/// Stores a route for a node.
		/// </summary>
		/// <param name="nodeId">The node identified.</param>
		/// <param name="route">The route.</param>
        public void Store(int nodeId, string route)
        {
            using (new WriteLock(_lock))
            {
                _routes[nodeId] = route;
                _nodeIds[route] = nodeId;
            }
        }

		/// <summary>
		/// Gets a route for a node.
		/// </summary>
		/// <param name="nodeId">The node identifier.</param>
		/// <returns>The route for the node, else null.</returns>
        public string GetRoute(int nodeId)
        {
            lock (new ReadLock(_lock))
            {
                return _routes.ContainsKey(nodeId) ? _routes[nodeId] : null;
            }
        }

		/// <summary>
		/// Gets a node for a route.
		/// </summary>
		/// <param name="route">The route.</param>
		/// <returns>The node identified for the route, else zero.</returns>
        public int GetNodeId(string route)
        {
            using (new ReadLock(_lock))
            {
                return _nodeIds.ContainsKey(route) ? _nodeIds[route] : 0;
            }
        }

		/// <summary>
		/// Clears the route for a node.
		/// </summary>
		/// <param name="nodeId">The node identifier.</param>
        public void ClearNode(int nodeId)
        {
            using (var lck = new UpgradeableReadLock(_lock))
            {
                if (_routes.ContainsKey(nodeId))
                {
					lck.UpgradeToWriteLock();
                    _nodeIds.Remove(_routes[nodeId]);
                    _routes.Remove(nodeId);
                }
            }
        }

		/// <summary>
		/// Clears all routes.
		/// </summary>
        public void Clear()
        {
            using (new WriteLock(_lock))
            {
                _routes = new Dictionary<int, string>();
                _nodeIds = new Dictionary<string, int>();
            }
        }
    }
}