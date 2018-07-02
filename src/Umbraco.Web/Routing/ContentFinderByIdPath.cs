﻿using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
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
        private readonly ILogger _logger;
        private readonly IWebRoutingSection _webRoutingSection;
        
        public ContentFinderByIdPath(IWebRoutingSection webRoutingSection, ILogger logger)
        {
            _webRoutingSection = webRoutingSection ?? throw new System.ArgumentNullException(nameof(webRoutingSection));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedContentRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TryFindContent(PublishedRequest frequest)
        {

            if (frequest.UmbracoContext != null && frequest.UmbracoContext.InPreviewMode == false
                && _webRoutingSection.DisableFindContentByIdPath)
                return false;

            IPublishedContent node = null;
            var path = frequest.Uri.GetAbsolutePathDecoded();

            var nodeId = -1;
            if (path != "/") // no id if "/"
            {
                var noSlashPath = path.Substring(1);

                if (int.TryParse(noSlashPath, out nodeId) == false)
                    nodeId = -1;

                if (nodeId > 0)
                {
                    _logger.Debug<ContentFinderByIdPath>(() => $"Id={nodeId}");
                    node = frequest.UmbracoContext.ContentCache.GetById(nodeId);

                    if (node != null)
                    {
                        frequest.PublishedContent = node;
                        _logger.Debug<ContentFinderByIdPath>(() => $"Found node with id={frequest.PublishedContent.Id}");
                    }
                    else
                    {
                        nodeId = -1; // trigger message below
                    }
                }
            }

            if (nodeId == -1)
                _logger.Debug<ContentFinderByIdPath>("Not a node id");

            return node != null;
        }
    }
}
