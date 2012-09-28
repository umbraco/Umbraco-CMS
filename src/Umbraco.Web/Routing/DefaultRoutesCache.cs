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
                // content - whenever the entire XML cache is rebuilt (from disk cache or from database)
                //  we must clear the cache entirely
				global::umbraco.content.AfterRefreshContent += (sender, e) => Clear();

                // document - whenever a document is updated in, or removed from, the XML cache
                //  we must clear the cache - at the moment, we clear the entire cache
                //  TODO could we do partial updates instead of clearing the whole cache?
				global::umbraco.content.AfterUpdateDocumentCache += (sender, e) => Clear();
                global::umbraco.content.AfterClearDocumentCache += (sender, e) => Clear();

                // domains - whenever a domain change we must clear the cache
                //  because routes contain the id of root nodes of domains
                //  TODO could we do partial updates instead of clearing the whole cache?
                global::umbraco.cms.businesslogic.web.Domain.AfterDelete += (sender, e) => Clear();
                global::umbraco.cms.businesslogic.web.Domain.AfterSave += (sender, e) => Clear();
                global::umbraco.cms.businesslogic.web.Domain.New += (sender, e) => Clear();

                // FIXME
                // the content class needs to be refactored - at the moment 
                // content.XmlContentInternal setter does not trigger any event
                // content.UpdateDocumentCache(List<Document> Documents) does not trigger any event
                // content.RefreshContentFromDatabaseAsync triggers AfterRefresh _while_ refreshing
                // etc...
                // in addition some events do not make sense... we trigger Publish when moving
                // a node, which we should not (the node is moved, not published...) etc.
			}			
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