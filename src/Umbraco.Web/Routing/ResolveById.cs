using System;
using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{

    // handles /1234 where 1234 is the id of a document
    //
    [ResolutionWeight(20)]
    internal class ResolveById : IRequestDocumentResolver
    {
		static readonly TraceSource Trace = new TraceSource("ResolveById");

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