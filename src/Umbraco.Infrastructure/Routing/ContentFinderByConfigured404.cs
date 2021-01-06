using System.Globalization;
using System.Linq;
using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentFinderByConfigured404"/> class.
        /// </summary>
        public ContentFinderByConfigured404(
            ILogger<ContentFinderByConfigured404> logger,
            IEntityService entityService,
            IOptions<ContentSettings> contentConfigSettings,
            IExamineManager examineManager,
            IUmbracoContextAccessor umbracoContextAccessor)
        {
            _logger = logger;
            _entityService = entityService;
            _contentSettings = contentConfigSettings.Value;
            _examineManager = examineManager;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TryFindContent(IPublishedRequestBuilder frequest)
        {
            IUmbracoContext umbCtx = _umbracoContextAccessor.UmbracoContext;
            if (umbCtx == null)
            {
                return false;
            }

            _logger.LogDebug("Looking for a page to handle 404.");

            // try to find a culture as best as we can
            CultureInfo errorCulture = CultureInfo.CurrentUICulture;
            if (frequest.Domain != null)
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
                    node = umbCtx.Content.GetByRoute(route, culture: frequest?.Culture?.Name);
                    if (node != null)
                    {
                        break;
                    }

                    pos = route.LastIndexOf('/');
                }

                if (node != null)
                {
                    Domain d = DomainUtilities.FindWildcardDomainInPath(umbCtx.PublishedSnapshot.Domains.GetAll(true), node.Path, null);
                    if (d != null)
                    {
                        errorCulture = d.Culture;
                    }
                }
            }

            var error404 = NotFoundHandlerHelper.GetCurrentNotFoundPageId(
                _contentSettings.Error404Collection.ToArray(),
                _entityService,
                new PublishedContentQuery(umbCtx.PublishedSnapshot, umbCtx.VariationContextAccessor, _examineManager),
                errorCulture);

            IPublishedContent content = null;

            if (error404.HasValue)
            {
                _logger.LogDebug("Got id={ErrorNodeId}.", error404.Value);

                content = umbCtx.Content.GetById(error404.Value);

                _logger.LogDebug(content == null
                    ? "Could not find content with that id."
                    : "Found corresponding content.");
            }
            else
            {
                _logger.LogDebug("Got nothing.");
            }

            frequest
                .SetPublishedContent(content)
                .SetIs404();

            return content != null;
        }
    }
}
