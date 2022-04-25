using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page URL rewrites
    /// that are stored when moving, saving, or deleting a node.
    /// </summary>
    /// <remarks>
    /// <para>Assigns a permanent redirect notification to the request.</para>
    /// </remarks>
    public partial class ContentFinderByRedirectUrl : IContentFinder
    {
        private readonly IRedirectUrlService _redirectUrlService;
        private readonly ILogger<ContentFinderByRedirectUrl> _logger;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;


        private static readonly Action<ILogger, string, Exception> s_logNoRouteMatch
            = LoggerMessage.Define<string>(MicrosoftLogLevel.Debug, new EventId(12), "No match for route: {Route}");

        private static readonly Action<ILogger, string, int, Exception> s_logRouteMatchedNoUrl
            = LoggerMessage.Define<string,int>(MicrosoftLogLevel.Debug, new EventId(13), "Route {Route} matches content {ContentId} which has no URL.");

        private static readonly Action<ILogger, string, int,string, Exception> s_logRouteMatchedRedirecting
            = LoggerMessage.Define<string, int,string>(MicrosoftLogLevel.Debug, new EventId(14), "Route {Route} matches content {ContentId} with URL '{Url}', redirecting.");

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentFinderByRedirectUrl"/> class.
        /// </summary>
        public ContentFinderByRedirectUrl(
            IRedirectUrlService redirectUrlService,
            ILogger<ContentFinderByRedirectUrl> logger,
            IPublishedUrlProvider publishedUrlProvider,
            IUmbracoContextAccessor umbracoContextAccessor)
        {
            _redirectUrlService = redirectUrlService;
            _logger = logger;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        /// <remarks>Optionally, can also assign the template or anything else on the document request, although that is not required.</remarks>
        public bool TryFindContent(IPublishedRequestBuilder frequest)
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return false;
            }

            var route = frequest.Domain != null
                ? frequest.Domain.ContentId + DomainUtilities.PathRelativeToDomain(frequest.Domain.Uri, frequest.AbsolutePathDecoded)
                : frequest.AbsolutePathDecoded;

            IRedirectUrl? redirectUrl = _redirectUrlService.GetMostRecentRedirectUrl(route, frequest.Culture);

            if (redirectUrl == null)
            {
                LogNoRouteMatch(route);
                return false;
            }

            IPublishedContent? content = umbracoContext.Content?.GetById(redirectUrl.ContentId);
            var url = content == null ? "#" : content.Url(_publishedUrlProvider, redirectUrl.Culture);
            if (url.StartsWith("#"))
            {
                LogRouteMatchedNoUrl( route, redirectUrl.ContentId);
                return false;
            }

            // Appending any querystring from the incoming request to the redirect URL
            url = string.IsNullOrEmpty(frequest.Uri.Query) ? url : url + frequest.Uri.Query;

            LogRouteMatchedRedirecting(route, content?.Id, url);

            frequest
                .SetRedirectPermanent(url)

                // From: http://stackoverflow.com/a/22468386/5018
                // See http://issues.umbraco.org/issue/U4-8361#comment=67-30532
                // Setting automatic 301 redirects to not be cached because browsers cache these very aggressively which then leads
                // to problems if you rename a page back to it's original name or create a new page with the original name
                .SetNoCacheHeader(true)
                .SetCacheExtensions(new List<string> { "no-store, must-revalidate" })
                .SetHeaders(new Dictionary<string, string> { { "Pragma", "no-cache" }, { "Expires", "0" } });

            return true;
        }

        private void LogNoRouteMatch(string route)
        {
            if (_logger.IsEnabled(MicrosoftLogLevel.Debug))
            {
                s_logNoRouteMatch.Invoke(_logger, route, null);
            }
        }

        private void LogRouteMatchedNoUrl(string route, int contentId)
        {
            if (_logger.IsEnabled(MicrosoftLogLevel.Debug))
            {
                s_logRouteMatchedNoUrl.Invoke(_logger, route, contentId, null);
            }
        }

        private void LogRouteMatchedRedirecting(string route, int contentId, string url)
        {
            if (_logger.IsEnabled(MicrosoftLogLevel.Debug))
            {
                s_logRouteMatchedRedirecting.Invoke(_logger, route, contentId, url, null);
            }
        }
    }
}
