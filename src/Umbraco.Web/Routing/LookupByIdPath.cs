using System;
using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IDocumentLookup"/> that handles page identifiers.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/1234</c> where <c>1234</c> is the identified of a document.</para>
	/// </remarks>
	//[ResolutionWeight(20)]
	internal class LookupByIdPath : IDocumentLookup
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TrySetDocument(DocumentRequest docRequest)
        {
            IDocument node = null;

            int nodeId = -1;
			if (docRequest.Uri.AbsolutePath != "/") // no id if "/"
            {
				string noSlashPath = docRequest.Uri.AbsolutePath.Substring(1);

                if (!Int32.TryParse(noSlashPath, out nodeId))
                    nodeId = -1;

                if (nodeId > 0)
                {
					LogHelper.Debug<LookupByIdPath>("Id={0}", () => nodeId);
					node = docRequest.RoutingContext.PublishedContentStore.GetDocumentById(
						docRequest.RoutingContext.UmbracoContext,
						nodeId);

                    if (node != null)
                    {
						docRequest.Document = node;
						LogHelper.Debug<LookupByIdPath>("Found node with id={0}", () => docRequest.DocumentId);
                    }
                    else
                    {
                        nodeId = -1; // trigger message below
                    }
                }
            }

            if (nodeId == -1)
				LogHelper.Debug<LookupByIdPath>("Not a node id");

            return node != null;
        }
    }
}