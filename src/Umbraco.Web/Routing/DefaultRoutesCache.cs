using System;
using System.Collections.Generic;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// The default implementation of IRoutesCache
	/// </summary>
	internal class DefaultRoutesCache : IRoutesCache
	{
        private readonly object _lock = new object();
        private Dictionary<int, string> _routes;
        private Dictionary<string, int> _nodeIds;

        public DefaultRoutesCache()
        {
            Clear();

            // here we should register handlers to clear the cache when content changes
            // at the moment this is done by library, which clears everything when content changed
            //
            // but really, we should do some partial refreshes!
            // otherwise, we could even cache 404 errors...
        }

        public void Store(int nodeId, string route)
        {
            lock (_lock)
            {
                _routes[nodeId] = route;
                _nodeIds[route] = nodeId;
            }
        }

        public string GetRoute(int nodeId)
        {
            lock (_lock)
            {
                return _routes.ContainsKey(nodeId) ? _routes[nodeId] : null;
            }
        }

        public int GetNodeId(string route)
        {
            lock (_lock)
            {
                return _nodeIds.ContainsKey(route) ? _nodeIds[route] : 0;
            }
        }

        public void ClearNode(int nodeId)
        {
            lock (_lock)
            {
                if (_routes.ContainsKey(nodeId))
                {
                    _nodeIds.Remove(_routes[nodeId]);
                    _routes.Remove(nodeId);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _routes = new Dictionary<int, string>();
                _nodeIds = new Dictionary<string, int>();
            }
        }
    }
}