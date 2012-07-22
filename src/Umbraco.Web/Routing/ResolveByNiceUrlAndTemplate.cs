using System.Diagnostics;
using System.Xml;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Web.Routing
{

    // handles /foo/bar/<template> where <template> is a valid template alias
    // and /foo/bar the nice url of a document
    //
    [RequestDocumentResolverWeight(30)]
    internal class ResolveByNiceUrlAndTemplate : ResolveByNiceUrl, IRequestDocumentResolver
    {
		static readonly TraceSource Trace = new TraceSource("ResolveByNiceUrlAndTemplate");		

        public override bool TrySetDocument(DocumentRequest docreq)
        {
            XmlNode node = null;
			string path = docreq.Uri.AbsolutePath;

			if (docreq.HasDomain)
				path = Domains.PathRelativeToDomain(docreq.DomainUri, path);
			if (path != "/") // no template if "/"
            {
				var pos = docreq.Uri.AbsolutePath.LastIndexOf('/');
				var templateAlias = docreq.Uri.AbsolutePath.Substring(pos + 1);
				path = path.Substring(0, pos);

                var template = Template.GetByAlias(templateAlias);
                if (template != null)
                {
                    Trace.TraceInformation("Valid template: \"{0}\"", templateAlias);

                    var route = docreq.HasDomain ? (docreq.Domain.RootNodeId.ToString() + path) : path;
                    node = LookupDocumentNode(docreq, route);

                    if (node != null)
                        docreq.Template = template;
                }
                else
                {
                    Trace.TraceInformation("Not a valid template: \"{0}\"", templateAlias);
                }
            }
            else
            {
                Trace.TraceInformation("No template in path \"/\"");
            }

            return node != null;
        }
    }
}