using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page identifiers.
    /// </summary>
    /// <remarks>
    /// <para>Handles <c>/1234</c> where <c>1234</c> is the identified of a document.</para>
    /// </remarks>
    public class ContentFinderByIdPath : IContentFinder
    {
        private readonly ILogger<ContentFinderByIdPath> _logger;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly WebRoutingSettings _webRoutingSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentFinderByIdPath"/> class.
        /// </summary>
        public ContentFinderByIdPath(
            IOptions<WebRoutingSettings> webRoutingSettings,
            ILogger<ContentFinderByIdPath> logger,
            IRequestAccessor requestAccessor,
            IUmbracoContextAccessor umbracoContextAccessor)
        {
            _webRoutingSettings = webRoutingSettings.Value ?? throw new System.ArgumentNullException(nameof(webRoutingSettings));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _requestAccessor = requestAccessor ?? throw new System.ArgumentNullException(nameof(requestAccessor));
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new System.ArgumentNullException(nameof(umbracoContextAccessor));
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TryFindContent(IPublishedRequestBuilder frequest)
        {
            IUmbracoContext umbCtx = _umbracoContextAccessor.UmbracoContext;
            if (umbCtx == null || (umbCtx != null && umbCtx.InPreviewMode == false && _webRoutingSettings.DisableFindContentByIdPath))
            {
                return false;
            }

            IPublishedContent node = null;
            var path = frequest.AbsolutePathDecoded;

            var nodeId = -1;

            // no id if "/"
            if (path != "/")
            {
                var noSlashPath = path.Substring(1);

                if (int.TryParse(noSlashPath, out nodeId) == false)
                {
                    nodeId = -1;
                }

                if (nodeId > 0)
                {
                    _logger.LogDebug("Id={NodeId}", nodeId);
                    node = umbCtx.Content.GetById(nodeId);

                    if (node != null)
                    {

                        var cultureFromQuerystring = _requestAccessor.GetQueryStringValue("culture");

                        // if we have a node, check if we have a culture in the query string
                        if (!string.IsNullOrEmpty(cultureFromQuerystring))
                        {
                            // we're assuming it will match a culture, if an invalid one is passed in, an exception will throw (there is no TryGetCultureInfo method), i think this is ok though
                            frequest.SetCulture(cultureFromQuerystring);
                        }

                        frequest.SetPublishedContent(node);
                        _logger.LogDebug("Found node with id={PublishedContentId}", node.Id);
                    }
                    else
                    {
                        nodeId = -1; // trigger message below
                    }
                }
            }

            if (nodeId == -1)
            {
                _logger.LogDebug("Not a node id");
            }

            return node != null;
        }
    }
}
