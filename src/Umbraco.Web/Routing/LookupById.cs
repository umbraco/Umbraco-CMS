using System;
using System.Diagnostics;
using System.Xml;

namespace Umbraco.Web.Routing
{

    // handles /1234 where 1234 is the id of a document
    //
    [LookupWeight(20)]
    internal class LookupById : ILookup
    {
        public LookupById(ContentStore contentStore)
        {
            _contentStore = contentStore;
        }

        static readonly TraceSource Trace = new TraceSource("LookupById");

        readonly ContentStore _contentStore;

        ////[Import]
        //IContentStore ContentStoreImport
        //{
        //    set { _contentStore = value; }
        //}

        //public LookupById()
        //{ }

        public bool LookupDocument(DocumentRequest docreq)
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
                    node = _contentStore.GetNodeById(nodeId);
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