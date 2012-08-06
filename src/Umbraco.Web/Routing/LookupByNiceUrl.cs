using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IDocumentLookup"/> that handles page nice urls.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/foo/bar</c> where <c>/foo/bar</c> is the nice url of a document.</para>
	/// </remarks>
	//[ResolutionWeight(10)]
    internal class LookupByNiceUrl : IDocumentLookup
    {
		
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docreq">The <c>DocumentRequest</c>.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public virtual bool TrySetDocument(DocumentRequest docreq)
        {
			string route;
			if (docreq.HasDomain)
				route = docreq.Domain.RootNodeId.ToString() + DomainHelper.PathRelativeToDomain(docreq.DomainUri, docreq.Uri.AbsolutePath);
			else
				route = docreq.Uri.AbsolutePath;

			//format the path
			route = route.Replace(".aspx", "");

            var node = LookupDocumentNode(docreq, route);
            return node != null;
        }

		/// <summary>
		/// Tries to find an Umbraco document for a <c>DocumentRequest</c> and a route.
		/// </summary>
		/// <param name="docreq">The document request.</param>
		/// <param name="route">The route.</param>
		/// <returns>The document node, or null.</returns>
        protected XmlNode LookupDocumentNode(DocumentRequest docreq, string route)
        {
			LogHelper.Debug<LookupByNiceUrl>("Test route \"{0}\"", () => route);

			//return '0' if in preview mode!
        	var nodeId = !docreq.UmbracoContext.InPreviewMode
							? docreq.UmbracoContext.RoutesCache.GetNodeId(route)
        	             	: 0;


            XmlNode node = null;
            if (nodeId > 0)
            {
				node = docreq.RoutingContext.ContentStore.GetNodeById(nodeId);
                if (node != null)
                {
                    docreq.Node = node;
					LogHelper.Debug<LookupByNiceUrl>("Cache hit, id={0}", () => nodeId);
                }
                else
                {
                    docreq.UmbracoContext.RoutesCache.ClearNode(nodeId);
                }
            }

            if (node == null)
            {
				LogHelper.Debug<LookupByNiceUrl>("Cache miss, query");
				node = docreq.RoutingContext.ContentStore.GetNodeByRoute(route);
                if (node != null)
                {
                    docreq.Node = node;
					LogHelper.Debug<LookupByNiceUrl>("Query matches, id={0}", () => docreq.NodeId);

					if (!docreq.UmbracoContext.InPreviewMode)
					{
						docreq.UmbracoContext.RoutesCache.Store(docreq.NodeId, route); // will not write if previewing	
					} 
                    
                }
                else
                {
					LogHelper.Debug<LookupByNiceUrl>("Query does not match");
                }
            }

            return node;
        }
    }
}