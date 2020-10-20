using System.Globalization;
using System.Linq;
using Examine;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that runs the legacy 404 logic.
    /// </summary>
    public class ContentFinderByConfigured404 : IContentLastChanceFinder
    {
        private readonly ILogger<ContentFinderByConfigured404> _logger;
        private readonly IEntityService _entityService;
        private readonly ContentSettings _contentSettings;
        private readonly IExamineManager _examineManager;

        public ContentFinderByConfigured404(
            ILogger<ContentFinderByConfigured404> logger,
            IEntityService entityService,
            IOptions<ContentSettings> contentConfigSettings,
            IExamineManager examineManager)
        {
            _logger = logger;
            _entityService = entityService;
            _contentSettings = contentConfigSettings.Value;
            _examineManager = examineManager;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TryFindContent(IPublishedRequest frequest)
        {
            _logger.LogDebug("Looking for a page to handle 404.");

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
                _contentSettings.Error404Collection.ToArray(),
                _entityService,
                new PublishedContentQuery(frequest.UmbracoContext.PublishedSnapshot, frequest.UmbracoContext.VariationContextAccessor, _examineManager),
                errorCulture);

            IPublishedContent content = null;

            if (error404.HasValue)
            {
                _logger.LogDebug("Got id={ErrorNodeId}.", error404.Value);

                content = frequest.UmbracoContext.Content.GetById(error404.Value);

                _logger.LogDebug(content == null
                    ? "Could not find content with that id."
                    : "Found corresponding content.");
            }
            else
            {
                _logger.LogDebug("Got nothing.");
            }

            frequest.PublishedContent = content;
            frequest.Is404 = true;
            return content != null;
        }
    }
}
