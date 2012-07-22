using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// The default implementation of IRoutesCache
	/// </summary>
	internal class DefaultRoutesCache : IRoutesCache
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
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
			//
			// these are the two events used by library in legacy code...
			// is it enough?
			//
			global::umbraco.content.AfterRefreshContent += (sender, e) => Clear();
			global::umbraco.content.AfterUpdateDocumentCache += (sender, e) => Clear();
        }

        public void Store(int nodeId, string route)
        {
            using (new WriteLock(_lock))
            {
                _routes[nodeId] = route;
                _nodeIds[route] = nodeId;
            }
        }

        public string GetRoute(int nodeId)
        {
            lock (new ReadLock(_lock))
            {
                return _routes.ContainsKey(nodeId) ? _routes[nodeId] : null;
            }
        }

        public int GetNodeId(string route)
        {
            using (new ReadLock(_lock))
            {
                return _nodeIds.ContainsKey(route) ? _nodeIds[route] : 0;
            }
        }

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