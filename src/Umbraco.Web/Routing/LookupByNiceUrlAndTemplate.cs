using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
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
	//[ResolutionWeight(30)]
    internal class LookupByNiceUrlAndTemplate : LookupByNiceUrl, IDocumentLookup
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>DocumentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>DocumentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		/// <remarks>If successful, also assigns the template.</remarks>
		public override bool TrySetDocument(DocumentRequest docRequest)
        {
            IDocument node = null;
			string path = docRequest.Uri.AbsolutePath;

			if (docRequest.HasDomain)
				path = DomainHelper.PathRelativeToDomain(docRequest.DomainUri, path);
			if (path != "/") // no template if "/"
            {
				var pos = docRequest.Uri.AbsolutePath.LastIndexOf('/');
				var templateAlias = docRequest.Uri.AbsolutePath.Substring(pos + 1);
				path = path.Substring(0, pos);

				//TODO: We need to check if the altTemplate is for MVC or not, though I'm not exactly sure how the best
				// way to do that would be since the template is just an alias and if we are not having a flag on the 
				// doc type for rendering engine and basing it only on template name, then how would we handle this?

                var template = Template.GetByAlias(templateAlias);
                if (template != null)
                {
					LogHelper.Debug<LookupByNiceUrlAndTemplate>("Valid template: \"{0}\"", () => templateAlias);

					var route = docRequest.HasDomain ? (docRequest.Domain.RootNodeId.ToString() + path) : path;
					node = LookupDocumentNode(docRequest, route);

                    if (node != null)
						docRequest.TemplateLookup = new TemplateLookup(template.Alias, template);
                }
                else
                {
					LogHelper.Debug<LookupByNiceUrlAndTemplate>("Not a valid template: \"{0}\"", () => templateAlias);
                }
            }
            else
            {
				LogHelper.Debug<LookupByNiceUrlAndTemplate>("No template in path \"/\"");
            }

            return node != null;
        }
    }
}