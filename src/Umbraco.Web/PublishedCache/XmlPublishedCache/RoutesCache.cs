using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    class RoutesCache
    {
        private ConcurrentDictionary<int, string> _routes;
        private ConcurrentDictionary<string, int> _nodeIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutesCache"/> class.
        /// </summary>
        public RoutesCache()
            : this(true)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutesCache"/> class.
        /// </summary>
        internal RoutesCache(bool bindToEvents)
		{
			Clear();

			if (bindToEvents)
			{
                Resolution.Frozen += ResolutionFrozen;
			}			
		}

        /// <summary>
        /// Once resolution is frozen, then we can bind to the events that we require
        /// </summary>
        /// <param name="s"></param>
        /// <param name="args"></param>
        private void ResolutionFrozen(object s, EventArgs args)
        {
            // content - whenever the entire XML cache is rebuilt (from disk cache or from database)
            //  we must clear the cache entirely
            global::umbraco.content.AfterRefreshContent += (sender, e) => Clear();

            // document - whenever a document is updated in, or removed from, the XML cache
            //  we must clear the cache - at the moment, we clear the entire cache
            global::umbraco.content.AfterUpdateDocumentCache += (sender, e) => Clear();
            global::umbraco.content.AfterClearDocumentCache += (sender, e) => Clear();

            // fixme - should refactor once content events are refactored
            // the content class needs to be refactored - at the moment 
            // content.XmlContentInternal setter does not trigger any event
            // content.UpdateDocumentCache(List<Document> Documents) does not trigger any event
            // content.RefreshContentFromDatabaseAsync triggers AfterRefresh _while_ refreshing
            // etc...
            // in addition some events do not make sense... we trigger Publish when moving
            // a node, which we should not (the node is moved, not published...) etc.
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
            if (!_routes.ContainsKey(nodeId)) return;

            string key;
            if (!_routes.TryGetValue(nodeId, out key)) return;

            int val;
            _nodeIds.TryRemove(key, out val);
            string val2;
            _routes.TryRemove(nodeId, out val2);
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
