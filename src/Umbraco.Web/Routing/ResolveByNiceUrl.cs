using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
    // handles "/foo/bar" where "/foo/bar" is the path to a document
    //

    [ResolutionWeight(10)]
    internal class ResolveByNiceUrl : IRequestDocumentResolver
    {
		static readonly TraceSource Trace = new TraceSource("ResolveByNiceUrl");

        public virtual bool TrySetDocument(DocumentRequest docreq)
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

			//return '0' if in preview mode!
        	var nodeId = !docreq.RoutingContext.UmbracoContext.InPreviewMode
							? docreq.RoutingContext.UmbracoContext.RoutesCache.GetNodeId(route)
        	             	: 0;


            XmlNode node = null;
            if (nodeId > 0)
            {
				node = docreq.RoutingContext.ContentStore.GetNodeById(nodeId);
                if (node != null)
                {
                    docreq.Node = node;
                    Trace.TraceInformation("Cache hit, id={0}", nodeId);
                }
                else
                {
                    docreq.RoutingContext.UmbracoContext.RoutesCache.ClearNode(nodeId);
                }
            }

            if (node == null)
            {
                Trace.TraceInformation("Cache miss, query");
				node = docreq.RoutingContext.ContentStore.GetNodeByRoute(route);
                if (node != null)
                {
                    docreq.Node = node;
                    Trace.TraceInformation("Query matches, id={0}", docreq.NodeId);

					if (!docreq.RoutingContext.UmbracoContext.InPreviewMode)
					{
						docreq.RoutingContext.UmbracoContext.RoutesCache.Store(docreq.NodeId, route); // will not write if previewing	
					} 
                    
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