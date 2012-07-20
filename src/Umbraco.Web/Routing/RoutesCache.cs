using System.Collections.Generic;

namespace Umbraco.Web.Routing
{
	// this is a bi-directional cache that contains
	// - nodeId to route (used for NiceUrl)
	// - route to nodeId (used for inbound requests)
	//
	// a route is [rootId]/path/to/node
	// where rootId is the id of the "site root" node
	// if missing then the "site root" is the content root
	//
    internal class RoutesCache
    {
        private readonly object _lock = new object();
        private Dictionary<int, string> _routes;
        private Dictionary<string, int> _nodeIds;

        private readonly UmbracoContext _umbracoContext;

        public RoutesCache(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;

            Clear();

            // here we should register handlers to clear the cache when content changes
            // at the moment this is done by library, which clears everything when content changed
            //
            // but really, we should do some partial refreshes!
            // otherwise, we could even cache 404 errors...
        }

        public void Store(int nodeId, string route)
        {
            if (_umbracoContext.InPreviewMode)
                return;

            lock (_lock)
            {
                _routes[nodeId] = route;
                _nodeIds[route] = nodeId;
            }
        }

        public string GetRoute(int nodeId)
        {
            if (_umbracoContext.InPreviewMode)
                return null;

            lock (_lock)
            {
                return _routes.ContainsKey(nodeId) ? _routes[nodeId] : null;
            }
        }

        public int GetNodeId(string route)
        {
            if (_umbracoContext.InPreviewMode)
                return 0;

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