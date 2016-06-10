using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page nice urls and a template.
    /// </summary>
    /// <remarks>
    /// <para>Handles <c>/foo/bar/template</c> where <c>/foo/bar</c> is the nice url of a document, and <c>template</c> a template alias.</para>
    /// <para>If successful, then the template of the document request is also assigned.</para>
    /// </remarks>
    public class ContentFinderByNiceUrlAndTemplate : ContentFinderByNiceUrl
    {
        public ContentFinderByNiceUrlAndTemplate(ILogger logger)
            : base(logger)
        { }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
        /// </summary>
        /// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        /// <remarks>If successful, also assigns the template.</remarks>
        public override bool TryFindContent(PublishedContentRequest docRequest)
        {
            IPublishedContent node = null;
            var path = docRequest.Uri.GetAbsolutePathDecoded();

            if (docRequest.HasDomain)
                path = DomainHelper.PathRelativeToDomain(docRequest.Domain.Uri, path);

            if (path != "/") // no template if "/"
            {
                var pos = path.LastIndexOf('/');
                var templateAlias = path.Substring(pos + 1);
                path = pos == 0 ? "/" : path.Substring(0, pos);

                var template = ApplicationContext.Current.Services.FileService.GetTemplate(templateAlias);
                if (template != null)
                {
                    Logger.Debug<ContentFinderByNiceUrlAndTemplate>("Valid template: \"{0}\"", () => templateAlias);

                    var route = docRequest.HasDomain ? (docRequest.Domain.ContentId.ToString() + path) : path;
                    node = FindContent(docRequest, route);

                    if (UmbracoConfig.For.UmbracoSettings().WebRouting.DisableAlternativeTemplates == false && node != null)
                        docRequest.TemplateModel = template;
                }
                else
                {
                    Logger.Debug<ContentFinderByNiceUrlAndTemplate>("Not a valid template: \"{0}\"", () => templateAlias);
                }
            }
            else
            {
                Logger.Debug<ContentFinderByNiceUrlAndTemplate>("No template in path \"/\"");
            }

            return node != null;
        }
    }
}