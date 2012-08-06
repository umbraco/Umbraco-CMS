using System;
using System.Collections.Concurrent;
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
		private ConcurrentDictionary<int, string> _routes;
		private ConcurrentDictionary<string, int> _nodeIds;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultRoutesCache"/> class.
		/// </summary>
        public DefaultRoutesCache() : this(true)
        {
            
        }

		internal DefaultRoutesCache(bool bindToEvents)
		{
			Clear();

			if (bindToEvents)
			{
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

			
		}

		/// <summary>
		/// Stores a route for a node.
		/// </summary>
		/// <param name="nodeId">The node identified.</param>
		/// <param name="route">The route.</param>
        public void Store(int nodeId, string route)
		{
			_routes.AddOrUpdate(nodeId, i => route, (i, s) => route);
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
			if (_routes.ContainsKey(nodeId))
			{
				string key;
				if (_routes.TryGetValue(nodeId, out key))
				{
					int val;
					_nodeIds.TryRemove(key, out val);
					string val2;
					_routes.TryRemove(nodeId, out val2);
				}				
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
    }
}