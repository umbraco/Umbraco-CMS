using System;
using System.Text;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page aliases.
    /// </summary>
    /// <remarks>
    /// <para>Handles <c>/just/about/anything</c> where <c>/just/about/anything</c> is contained in the <c>umbracoUrlAlias</c> property of a document.</para>
    /// <para>The alias is the full path to the document. There can be more than one alias, separated by commas.</para>
    /// </remarks>
    public class ContentFinderByUrlAlias : IContentFinder
    {
        private readonly IContentRouter _contentRouter;

        protected ILogger Logger { get; }

        public ContentFinderByUrlAlias(ILogger logger,IContentRouter contentRouter)
        {
            Logger = logger;
            _contentRouter = contentRouter;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        public bool TryFindContent(PublishedRequest frequest)
        {
            IPublishedContent node = null;

            if (frequest.Uri.AbsolutePath != "/") // no alias if "/"
            {
                var result = _contentRouter.GetIdByAlias(frequest.UmbracoContext.PublishedSnapshot,
                    frequest.UmbracoContext.InPreviewMode,
                    frequest.HasDomain ? frequest.Domain.ContentId : 0,
                    frequest.Culture.Name,
                    frequest.Uri.GetAbsolutePathDecoded());
                if(result.Outcome == RoutingOutcome.Found)
                {
                    node = frequest.UmbracoContext.Content.GetById(result.Id);
                }
                if (node != null)
                {
                    frequest.PublishedContent = node;
                    Logger.Debug<ContentFinderByUrlAlias>("Path '{UriAbsolutePath}' is an alias for id={PublishedContentId}", frequest.Uri.AbsolutePath, frequest.PublishedContent.Id);
                }
            }

            return node != null;
        }

    }
}
