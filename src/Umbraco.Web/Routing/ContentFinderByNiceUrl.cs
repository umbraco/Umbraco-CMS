using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.interfaces;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that handles page nice urls.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/foo/bar</c> where <c>/foo/bar</c> is the nice url of a document.</para>
	/// </remarks>
	internal class ContentFinderByNiceUrl : IContentFinder
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public virtual bool TryFindDocument(PublishedContentRequest docRequest)
        {
			string route;
			if (docRequest.HasDomain)
				route = docRequest.Domain.RootNodeId.ToString() + DomainHelper.PathRelativeToDomain(docRequest.DomainUri, docRequest.Uri.GetAbsolutePathDecoded());
			else
				route = docRequest.Uri.GetAbsolutePathDecoded();

			var node = LookupDocumentNode(docRequest, route);
            return node != null;
        }

		/// <summary>
		/// Tries to find an Umbraco document for a <c>PublishedContentRequest</c> and a route.
		/// </summary>
		/// <param name="docreq">The document request.</param>
		/// <param name="route">The route.</param>
		/// <returns>The document node, or null.</returns>
        protected IPublishedContent LookupDocumentNode(PublishedContentRequest docreq, string route)
        {
			LogHelper.Debug<ContentFinderByNiceUrl>("Test route \"{0}\"", () => route);

			// first ask the cache for a node
			// return '0' if in preview mode
        	var nodeId = !docreq.RoutingContext.UmbracoContext.InPreviewMode
							? docreq.RoutingContext.UmbracoContext.RoutingContext.RoutesCache.GetNodeId(route)
        	             	: 0;

			// if a node was found, get it by id and ensure it exists
			// else clear the cache
            IPublishedContent node = null;
            if (nodeId > 0)
            {
				node = docreq.RoutingContext.PublishedContentStore.GetDocumentById(
					docreq.RoutingContext.UmbracoContext,
					nodeId);

                if (node != null)
                {
                    docreq.PublishedContent = node;
					LogHelper.Debug<ContentFinderByNiceUrl>("Cache hit, id={0}", () => nodeId);
                }
                else
                {
                    docreq.RoutingContext.RoutesCache.ClearNode(nodeId);
                }
            }

			// if we still have no node, get it by route
            if (node == null)
            {
				LogHelper.Debug<ContentFinderByNiceUrl>("Cache miss, query");
				node = docreq.RoutingContext.PublishedContentStore.GetDocumentByRoute(
					docreq.RoutingContext.UmbracoContext,
					route);

                if (node != null)
                {
                    docreq.PublishedContent = node;
					LogHelper.Debug<ContentFinderByNiceUrl>("Query matches, id={0}", () => docreq.PublishedContent.Id);

					var iscanon = !DomainHelper.ExistsDomainInPath(docreq.Domain, node.Path);
					if (!iscanon)
						LogHelper.Debug<ContentFinderByNiceUrl>("Non canonical url");

					// do not store if previewing or if non-canonical
					if (!docreq.RoutingContext.UmbracoContext.InPreviewMode && iscanon)
						docreq.RoutingContext.RoutesCache.Store(docreq.PublishedContent.Id, route);
                    
                }
                else
                {
					LogHelper.Debug<ContentFinderByNiceUrl>("Query does not match");
                }
            }

            return node;
        }
    }
}