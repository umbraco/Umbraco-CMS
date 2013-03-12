using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that handles page aliases.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/just/about/anything</c> where <c>/just/about/anything</c> is contained in the <c>umbracoUrlAlias</c> property of a document.</para>
	/// <para>The alias is the full path to the document. There can be more than one alias, separated by commas.</para>
	/// </remarks>
	internal class ContentFinderByUrlAlias : IContentFinder
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindDocument(PublishedContentRequest docRequest)
		{
			IPublishedContent node = null;

			if (docRequest.Uri.AbsolutePath != "/") // no alias if "/"
			{
				node = docRequest.RoutingContext.PublishedContentStore.GetDocumentByUrlAlias(
					docRequest.RoutingContext.UmbracoContext, 
					docRequest.HasDomain ? docRequest.Domain.RootNodeId : 0, 
					docRequest.Uri.GetAbsolutePathDecoded());

				if (node != null)
				{					
					docRequest.PublishedContent = node;
					LogHelper.Debug<ContentFinderByUrlAlias>("Path \"{0}\" is an alias for id={1}", () => docRequest.Uri.AbsolutePath, () => docRequest.PublishedContent.Id);
				}
			}

			return node != null;
		}
    }
}