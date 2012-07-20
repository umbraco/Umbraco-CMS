using System.Diagnostics;
using System.Xml;

namespace Umbraco.Web.Routing
{
    // handles "/foo/bar" where "/foo/bar" is the path to a document
    //

    [LookupWeight(10)]
    internal class LookupByPath : ILookup
    {
        public LookupByPath(ContentStore contentStore, RoutesCache routesCache)
        {
            ContentStore = contentStore;
            RoutesCache = routesCache;
        }

        static readonly TraceSource Trace = new TraceSource("LookupByPath");

        protected ContentStore ContentStore;
        protected RoutesCache RoutesCache;

        public virtual bool LookupDocument(DocumentRequest docreq)
        {
			string route;
			if (docreq.HasDomain)
				route = docreq.Domain.RootNodeId.ToString() + Domains.PathRelativeToDomain(docreq.DomainUri, docreq.Uri.AbsolutePath);
			else
				route = docreq.Uri.AbsolutePath;

            var node = LookupDocumentNode(docreq, route);
            return node != null;
        }

        protected XmlNode LookupDocumentNode(DocumentRequest docreq, string route)
        {
            Trace.TraceInformation("Test route \"{0}\"", route);

            var nodeId = RoutesCache.GetNodeId(route);
            XmlNode node = null;
            if (nodeId > 0)
            {
                node = ContentStore.GetNodeById(nodeId);
                if (node != null)
                {
                    docreq.Node = node;
                    Trace.TraceInformation("Cache hit, id={0}", nodeId);
                }
                else
                {
                    RoutesCache.ClearNode(nodeId);
                }
            }

            if (node == null)
            {
                Trace.TraceInformation("Cache miss, query");
                node = ContentStore.GetNodeByRoute(route);
                if (node != null)
                {
                    docreq.Node = node;
                    Trace.TraceInformation("Query matches, id={0}", docreq.NodeId);
                    RoutesCache.Store(docreq.NodeId, route);
                }
                else
                {
                    Trace.TraceInformation("Query does not match");
                }
            }

            return node;
        }
    }
}