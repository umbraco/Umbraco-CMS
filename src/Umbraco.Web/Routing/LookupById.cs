using System;
using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IDocumentLookup"/> that handles page identifiers.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/1234</c> where <c>1234</c> is the identified of a document.</para>
	/// </remarks>
	//[ResolutionWeight(20)]
	internal class LookupById : IDocumentLookup
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TrySetDocument(DocumentRequest docRequest)
        {
            XmlNode node = null;

            int nodeId = -1;
			if (docRequest.Uri.AbsolutePath != "/") // no id if "/"
            {
				string noSlashPath = docRequest.Uri.AbsolutePath.Substring(1);

                if (!Int32.TryParse(noSlashPath, out nodeId))
                    nodeId = -1;

                if (nodeId > 0)
                {
					LogHelper.Debug<LookupById>("Id={0}", () => nodeId);
					node = docRequest.RoutingContext.ContentStore.GetNodeById(nodeId);
                    if (node != null)
                    {
						docRequest.XmlNode = node;
						LogHelper.Debug<LookupById>("Found node with id={0}", () => docRequest.NodeId);
                    }
                    else
                    {
                        nodeId = -1; // trigger message below
                    }
                }
            }

            if (nodeId == -1)
				LogHelper.Debug<LookupById>("Not a node id");

            return node != null;
        }
    }
}