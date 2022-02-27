using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page identifiers.
    /// </summary>
    /// <remarks>
    /// <para>Handles <c>/1234</c> where <c>1234</c> is the identified of a document.</para>
    /// </remarks>
    public partial class ContentFinderByIdPath : IContentFinder
    {
        private readonly ILogger<ContentFinderByIdPath> _logger;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly WebRoutingSettings _webRoutingSettings;

        private static readonly Action<ILogger, int,Exception> s_logNodeId
            = LoggerMessage.Define<int>(MicrosoftLogLevel.Debug, new EventId(10), "Id={NodeId}");

        private static readonly Action<ILogger, int, Exception> s_logFoundNodeId
            = LoggerMessage.Define<int>(MicrosoftLogLevel.Debug, new EventId(11), "Found node with id={PublishedContentId}");

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
            if(!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return false;
            }
            if (umbracoContext == null || (umbracoContext != null && umbracoContext.InPreviewMode == false && _webRoutingSettings.DisableFindContentByIdPath))
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

                if (int.TryParse(noSlashPath, NumberStyles.Integer, CultureInfo.InvariantCulture, out nodeId) == false)
                {
                    nodeId = -1;
                }

                if (nodeId > 0)
                {
                    LogNodeId(nodeId);
                    node = umbracoContext.Content.GetById(nodeId);

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
                        LogFoundNodeId(node.Id);
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

        private void LogNodeId(int nodeId)
        {
            if (_logger.IsEnabled(MicrosoftLogLevel.Debug))
            {
                s_logNodeId.Invoke(_logger, nodeId, null);
            }
        }

        private void LogFoundNodeId(int publishedContentId)
        {
            if (_logger.IsEnabled(MicrosoftLogLevel.Debug))
            {
                s_logFoundNodeId.Invoke(_logger, publishedContentId, null);
            }
        }
    }
}
