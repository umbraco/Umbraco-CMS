using System.Collections.Generic;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Routing
{
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
            var route = contentRequest.HasDomain
                ? contentRequest.UmbracoDomain.RootContentId + DomainHelper.PathRelativeToDomain(contentRequest.DomainUri, contentRequest.Uri.GetAbsolutePathDecoded())
                : contentRequest.Uri.GetAbsolutePathDecoded();

            var service = contentRequest.RoutingContext.UmbracoContext.Application.Services.RedirectUrlService;
            var redirectUrl = service.GetMostRecentRedirectUrl(route);

            // From: http://stackoverflow.com/a/22468386/5018
            // See http://issues.umbraco.org/issue/U4-8361#comment=67-30532
            // Setting automatic 301 redirects to not be cached because browsers cache these very aggressively which then leads 
            // to problems if you rename a page back to it's original name or create a new page with the original name
            contentRequest.Cacheability = HttpCacheability.NoCache;
            contentRequest.CacheExtensions = new List<string> { "no-store, must-revalidate" };
            contentRequest.Headers = new Dictionary<string, string> { { "Pragma", "no-cache" }, { "Expires", "0" } };

            if (redirectUrl == null)
            {
                LogHelper.Debug<ContentFinderByRedirectUrl>("No match for route: \"{0}\".", () => route);
                return false;
            }

            var content = contentRequest.RoutingContext.UmbracoContext.ContentCache.GetById(redirectUrl.ContentId);
            var url = content == null ? "#" : content.Url;
            if (url.StartsWith("#"))
            {
                LogHelper.Debug<ContentFinderByRedirectUrl>("Route \"{0}\" matches content {1} which has no url.",
                    () => route, () => redirectUrl.ContentId);
                return false;
            }

            // Apending any querystring from the incoming request to the redirect url.
            url = string.IsNullOrEmpty(contentRequest.Uri.Query) ? url : url + contentRequest.Uri.Query;

            LogHelper.Debug<ContentFinderByRedirectUrl>("Route \"{0}\" matches content {1} with url \"{2}\", redirecting.",
                () => route, () => content.Id, () => url);

            contentRequest.SetRedirectPermanent(url);
            return true;
        }
    }
}
