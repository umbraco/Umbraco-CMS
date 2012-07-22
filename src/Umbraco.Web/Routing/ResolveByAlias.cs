using System.Diagnostics;
using System.Xml;

namespace Umbraco.Web.Routing
{
    // handles /just/about/anything where /just/about/anything is the umbracoUrlAlias property of a document
    //
    // the alias is the full path to the document
    // there can be more than one alias, separated by commas
    // at the moment aliases are not cleaned up into nice urls
    //

    [RequestDocumentResolverWeight(50)]
    internal class ResolveByAlias : IRequestDocumentResolver
    {
		static readonly TraceSource Trace = new TraceSource("ResolveByAlias");


        public bool TrySetDocument(DocumentRequest docreq)
        {
            XmlNode node = null;

			if (docreq.Uri.AbsolutePath != "/") // no alias if "/"
            {
                node = docreq.RoutingContext.ContentStore.GetNodeByUrlAlias(docreq.HasDomain ? docreq.Domain.RootNodeId : 0, docreq.Uri.AbsolutePath);
                if (node != null)
                {
                    Trace.TraceInformation("Path \"{0}\" is an alias for id={1}", docreq.Uri.AbsolutePath, docreq.NodeId);
                    docreq.Node = node;
                }
            }

            if (node == null)
                Trace.TraceInformation("Not an alias");

            return node != null;
        }
    }
}