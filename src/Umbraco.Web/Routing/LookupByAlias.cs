using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Resolving;

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
		/// <param name="docreq">The <c>DocumentRequest</c>.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TrySetDocument(DocumentRequest docreq)
        {
            XmlNode node = null;

			if (docreq.Uri.AbsolutePath != "/") // no alias if "/"
            {
                node = docreq.RoutingContext.ContentStore.GetNodeByUrlAlias(docreq.HasDomain ? docreq.Domain.RootNodeId : 0, docreq.Uri.AbsolutePath);
                if (node != null)
                {
					LogHelper.Debug<LookupByAlias>("Path \"{0}\" is an alias for id={1}", () => docreq.Uri.AbsolutePath, () => docreq.NodeId);
                    docreq.XmlNode = node;
                }
            }

            if (node == null)
				LogHelper.Debug<LookupByAlias>("Not an alias");

            return node != null;
        }
    }
}