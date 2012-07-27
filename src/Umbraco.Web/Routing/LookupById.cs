using System;
using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IDocumentLookup"/> that handles page identifiers.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/1234</c> where <c>1234</c> is the identified of a document.</para>
	/// </remarks>
	[ResolutionWeight(20)]
    internal class LookupById : IDocumentLookup
    {
		static readonly TraceSource Trace = new TraceSource("LookupById");

		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TrySetDocument(DocumentRequest docreq)
        {
            XmlNode node = null;

            int nodeId = -1;
			if (docreq.Uri.AbsolutePath != "/") // no id if "/"
            {
				string noSlashPath = docreq.Uri.AbsolutePath.Substring(1);

                if (!Int32.TryParse(noSlashPath, out nodeId))
                    nodeId = -1;

                if (nodeId > 0)
                {
                    Trace.TraceInformation("Id={0}", nodeId);
					node = docreq.RoutingContext.ContentStore.GetNodeById(nodeId);
                    if (node != null)
                    {
                        docreq.Node = node;
                        Trace.TraceInformation("Found node with id={0}", docreq.NodeId);
                    }
                    else
                    {
                        nodeId = -1; // trigger message below
                    }
                }
            }

            if (nodeId == -1)
                Trace.TraceInformation("Not a node id");

            return node != null;
        }
    }
}