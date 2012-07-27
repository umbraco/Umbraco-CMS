using System.Diagnostics;
using System.Xml;
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
    [ResolutionWeight(50)]
    internal class LookupByAlias : IDocumentLookup
    {
		static readonly TraceSource Trace = new TraceSource("LookupByAlias");

		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
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