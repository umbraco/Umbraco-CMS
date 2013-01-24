using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that handles page nice urls.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/foo/bar</c> where <c>/foo/bar</c> is the nice url of a document.</para>
	/// </remarks>
    public class ContentFinderByNiceUrl : IContentFinder
    {
	    /// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public virtual bool TryFindContent(PublishedContentRequest docRequest)
        {
			string route;
			if (docRequest.HasDomain)
				route = docRequest.Domain.RootNodeId.ToString() + DomainHelper.PathRelativeToDomain(docRequest.DomainUri, docRequest.Uri.GetAbsolutePathDecoded());
			else
				route = docRequest.Uri.GetAbsolutePathDecoded();

			var node = FindContent(docRequest, route);
            return node != null;
        }

		/// <summary>
		/// Tries to find an Umbraco document for a <c>PublishedContentRequest</c> and a route.
		/// </summary>
		/// <param name="docreq">The document request.</param>
		/// <param name="route">The route.</param>
		/// <returns>The document node, or null.</returns>
        protected IPublishedContent FindContent(PublishedContentRequest docreq, string route)
        {
			LogHelper.Debug<ContentFinderByNiceUrl>("Test route \"{0}\"", () => route);

		    var node = docreq.RoutingContext.UmbracoContext.ContentCache.GetByRoute(route);
            if (node != null)
            {
                docreq.PublishedContent = node;
                LogHelper.Debug<ContentFinderByNiceUrl>("Got content, id={0}", () => node.Id);
            }
            else
            {
                LogHelper.Debug<ContentFinderByNiceUrl>("No match.");
            }

		    return node;
        }
    }
}