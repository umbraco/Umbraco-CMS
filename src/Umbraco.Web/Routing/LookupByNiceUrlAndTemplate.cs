using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Resolving;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IDocumentLookup"/> that handles page nice urls and a template.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/foo/bar/template</c> where <c>/foo/bar</c> is the nice url of a document, and <c>template</c> a template alias.</para>
	/// <para>If successful, then the template of the document request is also assigned.</para>
	/// </remarks>
	[ResolutionWeight(30)]
    internal class LookupByNiceUrlAndTemplate : LookupByNiceUrl, IDocumentLookup
    {
		static readonly TraceSource Trace = new TraceSource("LookupByNiceUrlAndTemplate");

		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		/// <remarks>If successful, also assigns the template.</remarks>
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