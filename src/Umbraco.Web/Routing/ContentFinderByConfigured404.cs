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
    public class ContentFinderByConfigured404 : IContentLastChanceFinder
    {
        private readonly ILogger _logger;
        private readonly IEntityService _entityService;
        private readonly IContentSection _contentConfigSection;

        public ContentFinderByConfigured404(ILogger logger, IEntityService entityService, IContentSection contentConfigSection)
        {
            _logger = logger;
            _entityService = entityService;
            _contentConfigSection = contentConfigSection;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TryFindContent(PublishedRequest frequest)
        {
            _logger.Debug<ContentFinderByConfigured404>("Looking for a page to handle 404.");

            int? domainConentId = null;

            // try to find a culture as best as we can
            var errorCulture = CultureInfo.CurrentUICulture;
            if (frequest.HasDomain)
            {
                errorCulture = frequest.Domain.Culture;
                domainConentId = frequest.Domain.ContentId;
            }
            else
            {
                var route = frequest.Uri.GetAbsolutePathDecoded();
                var pos = route.LastIndexOf('/');
                IPublishedContent node = null;
                while (pos > 1)
                {
                    route = route.Substring(0, pos);
                    node = frequest.UmbracoContext.Content.GetByRoute(route, culture: frequest?.Culture?.Name);
                    if (node != null) break;
                    pos = route.LastIndexOf('/');
                }
                if (node != null)
                {
                    var d = DomainUtilities.FindWildcardDomainInPath(frequest.UmbracoContext.PublishedSnapshot.Domains.GetAll(true), node.Path, null);
                    if (d != null)
                        errorCulture = d.Culture;
                }
            }

            var error404 = NotFoundHandlerHelper.GetCurrentNotFoundPageId(
                _contentConfigSection.Error404Collection.ToArray(),
                _entityService,
                new PublishedContentQuery(frequest.UmbracoContext.PublishedSnapshot, frequest.UmbracoContext.VariationContextAccessor),
                errorCulture,
                domainConentId
                );

            IPublishedContent content = null;

            if (error404.HasValue)
            {
                _logger.Debug<ContentFinderByConfigured404,int>("Got id={ErrorNodeId}.", error404.Value);

                content = frequest.UmbracoContext.Content.GetById(error404.Value);

                _logger.Debug<ContentFinderByConfigured404>(content == null
                    ? "Could not find content with that id."
                    : "Found corresponding content.");
            }
            else
            {
                _logger.Debug<ContentFinderByConfigured404>("Got nothing.");
            }

            frequest.PublishedContent = content;
            frequest.Is404 = true;
            return content != null;
        }
    }
}
