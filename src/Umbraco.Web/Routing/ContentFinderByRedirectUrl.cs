namespace Umbraco.Web.Routing
{
    using Umbraco.Core;
    using Umbraco.Core.Logging;

    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page url rewrites
    /// that are stored when moving, saving, or deleting a node.
    /// </summary>
    /// <remarks>
    /// <para>Assigns a permanent redirect notification to the request.</para>
    /// </remarks>
    public class ContentFinderByRedirectUrl : IContentFinder
    {
        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
        /// </summary>
        /// <param name="contentRequest">The <c>PublishedContentRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        /// <remarks>Optionally, can also assign the template or anything else on the document request, although that is not required.</remarks>
        public bool TryFindContent(PublishedContentRequest contentRequest)
        {
            string route;
            if (contentRequest.HasDomain)
            {
                route = contentRequest.UmbracoDomain.RootContentId + DomainHelper.PathRelativeToDomain(contentRequest.DomainUri, contentRequest.Uri.GetAbsolutePathDecoded());
            }
            else
            {
                route = contentRequest.Uri.GetAbsolutePathDecoded();
            }

            return this.FindContent(contentRequest, route);
        }

        /// <summary>
        /// Tries to find an Umbraco document for a <c>PublishedContentRequest</c> and a route.
        /// </summary>
        /// <param name="contentRequest">The document request.</param>
        /// <param name="route">The route.</param>
        /// <returns>True if a redirect is to take place, otherwise; false.</returns>
        protected bool FindContent(PublishedContentRequest contentRequest, string route)
        {
            var rule = contentRequest.RoutingContext
                                     .UmbracoContext.Application.Services
                                     .RedirectUrlService.GetMostRecentRule(route); 

            if (rule != null)
            {
                var content = contentRequest.RoutingContext.UmbracoContext.ContentCache.GetById(rule.ContentId);
                if (content != null)
                {
                    var url = content.Url;
                    if (url != "#")
                    {
                        contentRequest.SetRedirectPermanent(url);
                        LogHelper.Debug<ContentFinderByRedirectUrl>("Got content, id={0}", () => content.Id);
                        return true;
                    }
                }
            }

            LogHelper.Debug<ContentFinderByRedirectUrl>("No match for the url: {0}.", () => route);
            return false;
        }
    }
}
