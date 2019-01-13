using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that runs the legacy 404 logic.
    /// </summary>
    public class ContentFinderByLegacy404 : IContentLastChanceFinder
    {

        private readonly ILogger _logger;
        private readonly IEntityService _entityService;
        private readonly IContentSection _contentConfigSection;

        public ContentFinderByLegacy404(ILogger logger, IEntityService entityService, IContentSection contentConfigSection)
        {
            _logger = logger;
            _entityService = entityService;
            _contentConfigSection = contentConfigSection;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedContentRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TryFindContent(PublishedRequest frequest)
        {
            _logger.Debug<ContentFinderByLegacy404>("Looking for a page to handle 404.");

            // try to find a culture as best as we can
            var errorCulture = CultureInfo.CurrentUICulture;
            if (frequest.HasDomain)
            {
                errorCulture = frequest.Domain.Culture;
            }
            else
            {
                var route = frequest.Uri.GetAbsolutePathDecoded();
                var pos = route.LastIndexOf('/');
                IPublishedContent node = null;
                while (pos > 1)
                {
                    route = route.Substring(0, pos);
                    node = frequest.UmbracoContext.ContentCache.GetByRoute(route, culture: frequest?.Culture?.Name);
                    if (node != null) break;
                    pos = route.LastIndexOf('/');
                }
                if (node != null)
                {
                    var d = DomainHelper.FindWildcardDomainInPath(frequest.UmbracoContext.PublishedSnapshot.Domains.GetAll(true), node.Path, null);
                    if (d != null)
                        errorCulture = d.Culture;
                }
            }

            var error404 = NotFoundHandlerHelper.GetCurrentNotFoundPageId(
                _contentConfigSection.Error404Collection.ToArray(),
                _entityService,
                new PublishedContentQuery(frequest.UmbracoContext.ContentCache, frequest.UmbracoContext.MediaCache, frequest.UmbracoContext.VariationContextAccessor),
                errorCulture);

            IPublishedContent content = null;

            if (error404.HasValue)
            {
                _logger.Debug<ContentFinderByLegacy404>("Got id={ErrorNodeId}.", error404.Value);

                content = frequest.UmbracoContext.ContentCache.GetById(error404.Value);

                _logger.Debug<ContentFinderByLegacy404>(content == null
                    ? "Could not find content with that id."
                    : "Found corresponding content.");
            }
            else
            {
                _logger.Debug<ContentFinderByLegacy404>("Got nothing.");
            }

            frequest.PublishedContent = content;
            frequest.Is404 = true;
            return content != null;
        }
    }
}
