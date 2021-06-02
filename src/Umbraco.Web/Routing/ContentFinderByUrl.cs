using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page nice URLs.
    /// </summary>
    /// <remarks>
    /// <para>Handles <c>/foo/bar</c> where <c>/foo/bar</c> is the nice URL of a document.</para>
    /// </remarks>
    public class ContentFinderByUrl : IContentFinder
    {
        protected ILogger Logger { get; }

        public ContentFinderByUrl(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public virtual bool TryFindContent(PublishedRequest frequest)
        {
            string route;
            if (frequest.HasDomain)
                route = frequest.Domain.ContentId + DomainUtilities.PathRelativeToDomain(frequest.Domain.Uri, frequest.Uri.GetAbsolutePathDecoded());
            else
                route = frequest.Uri.GetAbsolutePathDecoded();

            var node = FindContent(frequest, route);
            return node != null;
        }

        /// <summary>
        /// Tries to find an Umbraco document for a <c>PublishedRequest</c> and a route.
        /// </summary>
        /// <param name="docreq">The document request.</param>
        /// <param name="route">The route.</param>
        /// <returns>The document node, or null.</returns>
        protected IPublishedContent FindContent(PublishedRequest docreq, string route)
        {
            if (docreq == null) throw new System.ArgumentNullException(nameof(docreq));

            Logger.Debug<ContentFinderByUrl, string>("Test route {Route}", route);

            var node = docreq.UmbracoContext.Content.GetByRoute(docreq.UmbracoContext.InPreviewMode, route, culture: docreq.Culture?.Name);
            if (node != null)
            {
                docreq.PublishedContent = node;
                Logger.Debug<ContentFinderByUrl,int>("Got content, id={NodeId}", node.Id);
            }
            else
            {
                Logger.Debug<ContentFinderByUrl>("No match.");
            }

            return node;
        }
    }
}
