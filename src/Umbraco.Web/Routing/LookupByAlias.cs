using System.Diagnostics;
using System.Xml;

namespace Umbraco.Web.Routing
{
    // handles /just/about/anything where /just/about/anything is the umbracoUrlAlias property of a document
    //
    // the alias is the full path to the document
    // there can be more than one alias, separated by commas
    // at the moment aliases are not cleaned up into nice urls
    //

    [LookupWeight(50)]
    internal class LookupByAlias : ILookup
    {
        public LookupByAlias(ContentStore contentStore)
        {
            _contentStore = contentStore;
        }

        static readonly TraceSource Trace = new TraceSource("LookupByAlias");

        readonly ContentStore _contentStore;

        public bool LookupDocument(DocumentRequest docreq)
        {
            XmlNode node = null;

            if (docreq.Path != "/") // no alias if "/"
            {
                node = _contentStore.GetNodeByUrlAlias(docreq.HasDomain ? docreq.Domain.RootNodeId : 0, docreq.Path);
                if (node != null)
                {
                    Trace.TraceInformation("Path \"{0}\" is an alias for id={1}", docreq.Path, docreq.NodeId);
                    docreq.Node = node;
                }
            }

            if (node == null)
                Trace.TraceInformation("Not an alias");

            return node != null;
        }
    }
}