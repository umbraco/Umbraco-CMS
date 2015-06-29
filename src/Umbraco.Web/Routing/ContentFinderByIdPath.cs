using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that handles page identifiers.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/1234</c> where <c>1234</c> is the identified of a document.</para>
	/// </remarks>
	public class ContentFinderByIdPath : IContentFinder
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindContent(PublishedContentRequest docRequest)
        {
            IPublishedContent node = null;
			var path = docRequest.Uri.GetAbsolutePathDecoded();

            var nodeId = -1;
			if (path != "/") // no id if "/"
            {
				var noSlashPath = path.Substring(1);

                if (!Int32.TryParse(noSlashPath, out nodeId))
                    nodeId = -1;

                if (nodeId > 0)
                {
					LogHelper.Debug<ContentFinderByIdPath>("Id={0}", () => nodeId);
                    node = docRequest.RoutingContext.UmbracoContext.ContentCache.GetById(nodeId);

                    if (node != null)
                    {
						docRequest.PublishedContent = node;
						LogHelper.Debug<ContentFinderByIdPath>("Found node with id={0}", () => docRequest.PublishedContent.Id);
                    }
                    else
                    {
                        nodeId = -1; // trigger message below
                    }
                }
            }

            if (nodeId == -1)
				LogHelper.Debug<ContentFinderByIdPath>("Not a node id");

            return node != null;
        }
    }
}