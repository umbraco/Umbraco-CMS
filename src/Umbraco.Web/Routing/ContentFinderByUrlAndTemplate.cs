using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Core.Services;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page nice urls and a template.
    /// </summary>
    /// <remarks>
    /// <para>This finder allows for an odd routing pattern similar to altTemplate, probably only use case is if there is an alternative mime type template and it should be routable by something like "/hello/world/json" where the JSON template is to be used for the "world" page</para>
    /// <para>Handles <c>/foo/bar/template</c> where <c>/foo/bar</c> is the nice url of a document, and <c>template</c> a template alias.</para>
    /// <para>If successful, then the template of the document request is also assigned.</para>
    /// </remarks>
    public class ContentFinderByUrlAndTemplate : ContentFinderByUrl
    {
        private readonly IFileService _fileService;

        public ContentFinderByUrlAndTemplate(ILogger logger, IFileService fileService)
            : base(logger)
        {
            _fileService = fileService;
        }

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

                var template = _fileService.GetTemplate(templateAlias);
                if (template != null)
                {
                    Logger.Debug<ContentFinderByUrlAndTemplate>("Valid template: '{TemplateAlias}'", templateAlias);

                    var route = frequest.HasDomain ? (frequest.Domain.ContentId.ToString() + path) : path;
                    node = FindContent(frequest, route);

                    if (UmbracoConfig.For.UmbracoSettings().WebRouting.DisableAlternativeTemplates == false && node != null)
                        frequest.TemplateModel = template;
                }
                else
                {
                    Logger.Debug<ContentFinderByUrlAndTemplate>("Not a valid template: '{TemplateAlias}'", templateAlias);
                }
            }
            else
            {
                Logger.Debug<ContentFinderByUrlAndTemplate>("No template in path '/'");
            }

            return node != null;
        }
    }
}
