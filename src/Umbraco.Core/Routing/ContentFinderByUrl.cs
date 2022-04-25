using System;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page nice URLs.
    /// </summary>
    /// <remarks>
    /// <para>Handles <c>/foo/bar</c> where <c>/foo/bar</c> is the nice URL of a document.</para>
    /// </remarks>
    public partial class ContentFinderByUrl : IContentFinder
    {
        private readonly ILogger<ContentFinderByUrl> _logger;

        private static readonly Action<ILogger, string, Exception> s_logTestRoute
            = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(15), "Test route {Route}");

        private static readonly Action<ILogger, int, Exception> s_logContentFound
            = LoggerMessage.Define<int>(LogLevel.Debug, new EventId(16), "Got content, id={NodeId}");

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentFinderByUrl"/> class.
        /// </summary>
        public ContentFinderByUrl(ILogger<ContentFinderByUrl> logger, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            UmbracoContextAccessor = umbracoContextAccessor ?? throw new System.ArgumentNullException(nameof(umbracoContextAccessor));
        }

        /// <summary>
        /// Gets the <see cref="IUmbracoContextAccessor"/>
        /// </summary>
        protected IUmbracoContextAccessor UmbracoContextAccessor { get; }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public virtual bool TryFindContent(IPublishedRequestBuilder frequest)
        {
            if (!UmbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return false;
            }

            string route;
            if (frequest.Domain != null)
            {
                route = frequest.Domain.ContentId + DomainUtilities.PathRelativeToDomain(frequest.Domain.Uri, frequest.AbsolutePathDecoded);
            }
            else
            {
                route = frequest.AbsolutePathDecoded;
            }

            IPublishedContent? node = FindContent(frequest, route);
            return node != null;
        }

        /// <summary>
        /// Tries to find an Umbraco document for a <c>PublishedRequest</c> and a route.
        /// </summary>
        /// <returns>The document node, or null.</returns>
        protected IPublishedContent? FindContent(IPublishedRequestBuilder docreq, string route)
        {
            if (!UmbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return null;
            }

            if (docreq == null)
            {
                throw new System.ArgumentNullException(nameof(docreq));
            }

            LogTestRoute(route);

            IPublishedContent? node = umbracoContext.Content?.GetByRoute(umbracoContext.InPreviewMode, route, culture: docreq.Culture);
            if (node != null)
            {
                docreq.SetPublishedContent(node);
                LogContentFound(node.Id);
            }
            else
            {
                _logger.LogDebug("No match.");
            }

            return node;
        }

        private void LogTestRoute(string route)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logTestRoute.Invoke(_logger, route, null);
            }
        }

        private void LogContentFound(int nodeId)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logContentFound.Invoke(_logger, nodeId, null);
            }
        }
    }
}
