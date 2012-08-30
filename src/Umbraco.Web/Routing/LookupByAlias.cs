using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IDocumentLookup"/> that handles page aliases.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/just/about/anything</c> where <c>/just/about/anything</c> is contained in the <c>umbracoUrlAlias</c> property of a document.</para>
	/// <para>The alias is the full path to the document. There can be more than one alias, separated by commas.</para>
	/// </remarks>
	//[ResolutionWeight(50)]
	internal class LookupByAlias : IDocumentLookup
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TrySetDocument(DocumentRequest docRequest)
		{
			
			IDocument node = null;

			if (docRequest.Uri.AbsolutePath != "/") // no alias if "/"
			{
				node = docRequest.RoutingContext.PublishedContentStore.GetDocumentByUrlAlias(
					docRequest.RoutingContext.UmbracoContext, 
					docRequest.HasDomain ? docRequest.Domain.RootNodeId : 0, 
					docRequest.Uri.AbsolutePath);

				if (node != null)
				{					
					docRequest.Document = node;
					LogHelper.Debug<LookupByAlias>("Path \"{0}\" is an alias for id={1}", () => docRequest.Uri.AbsolutePath, () => docRequest.DocumentId);
				}
			}

			return node != null;
		}
    }
}