using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.interfaces;

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
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public virtual bool TrySetDocument(DocumentRequest docRequest)
        {
			string route;
			if (docRequest.HasDomain)
				route = docRequest.Domain.RootNodeId.ToString() + DomainHelper.PathRelativeToDomain(docRequest.DomainUri, docRequest.Uri.AbsolutePath);
			else
				route = docRequest.Uri.AbsolutePath;
			
			var node = LookupDocumentNode(docRequest, route);
            return node != null;
        }

		/// <summary>
		/// Tries to find an Umbraco document for a <c>DocumentRequest</c> and a route.
		/// </summary>
		/// <param name="docreq">The document request.</param>
		/// <param name="route">The route.</param>
		/// <returns>The document node, or null.</returns>
        protected IDocument LookupDocumentNode(DocumentRequest docreq, string route)
        {
			LogHelper.Debug<LookupByNiceUrl>("Test route \"{0}\"", () => route);

			//return '0' if in preview mode!
        	var nodeId = !docreq.RoutingContext.UmbracoContext.InPreviewMode
							? docreq.RoutingContext.UmbracoContext.RoutesCache.GetNodeId(route)
        	             	: 0;


            IDocument node = null;
            if (nodeId > 0)
            {
				node = docreq.RoutingContext.PublishedContentStore.GetDocumentById(
					docreq.RoutingContext.UmbracoContext,
					nodeId);

                if (node != null)
                {
                    docreq.Document = node;
					LogHelper.Debug<LookupByNiceUrl>("Cache hit, id={0}", () => nodeId);
                }
                else
                {
                    docreq.RoutingContext.UmbracoContext.RoutesCache.ClearNode(nodeId);
                }
            }

            if (node == null)
            {
				LogHelper.Debug<LookupByNiceUrl>("Cache miss, query");
				node = docreq.RoutingContext.PublishedContentStore.GetDocumentByRoute(
					docreq.RoutingContext.UmbracoContext,
					route);

                if (node != null)
                {
                    docreq.Document = node;
					LogHelper.Debug<LookupByNiceUrl>("Query matches, id={0}", () => docreq.DocumentId);

					if (!docreq.RoutingContext.UmbracoContext.InPreviewMode)
					{
						docreq.RoutingContext.UmbracoContext.RoutesCache.Store(docreq.DocumentId, route); // will not write if previewing	
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