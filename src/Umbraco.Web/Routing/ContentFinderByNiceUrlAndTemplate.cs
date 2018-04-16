using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;

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
        /// <param name="frequest">The <c>PublishedContentRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        /// <remarks>If successful, also assigns the template.</remarks>
        public override bool TryFindContent(PublishedRequest frequest)
        {
            IPublishedContent node = null;
            var path = frequest.Uri.GetAbsolutePathDecoded();

            if (frequest.HasDomain)
                path = DomainHelper.PathRelativeToDomain(frequest.Domain.Uri, path);

            if (path != "/") // no template if "/"
            {
                var pos = path.LastIndexOf('/');
                var templateAlias = path.Substring(pos + 1);
                path = pos == 0 ? "/" : path.Substring(0, pos);

                var template = Current.Services.FileService.GetTemplate(templateAlias);
                if (template != null)
                {
                    Logger.Debug<ContentFinderByNiceUrlAndTemplate>(() => $"Valid template: \"{templateAlias}\"");

                    var route = frequest.HasDomain ? (frequest.Domain.ContentId.ToString() + path) : path;
                    node = FindContent(frequest, route);

                    if (UmbracoConfig.For.UmbracoSettings().WebRouting.DisableAlternativeTemplates == false && node != null)
                        frequest.TemplateModel = template;
                }
                else
                {
                    Logger.Debug<ContentFinderByNiceUrlAndTemplate>(() => $"Not a valid template: \"{templateAlias}\"");
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
