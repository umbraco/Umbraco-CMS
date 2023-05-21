using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides an implementation of <see cref="IContentFinder" /> that handles page URL rewrites
///     that are stored when moving, saving, or deleting a node.
/// </summary>
/// <remarks>
///     <para>Assigns a permanent redirect notification to the request.</para>
/// </remarks>
public class ContentFinderByRedirectUrl : IContentFinder
{
    private readonly ILogger<ContentFinderByRedirectUrl> _logger;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByRedirectUrl" /> class.
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
    ///     Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
    /// </summary>
    /// <param name="frequest">The <c>PublishedRequest</c>.</param>
    /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
    /// <remarks>
    ///     Optionally, can also assign the template or anything else on the document request, although that is not
    ///     required.
    /// </remarks>
    public async Task<bool> TryFindContent(IPublishedRequestBuilder frequest)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
        {
            return false;
        }

        var route = frequest.Domain != null
            ? frequest.Domain.ContentId +
              DomainUtilities.PathRelativeToDomain(frequest.Domain.Uri, frequest.AbsolutePathDecoded)
            : frequest.AbsolutePathDecoded;

        IRedirectUrl? redirectUrl = await _redirectUrlService.GetMostRecentRedirectUrlAsync(route, frequest.Culture);

        if (redirectUrl == null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No match for route: {Route}", route);
            }

            return false;
        }

        IPublishedContent? content = umbracoContext.Content?.GetById(redirectUrl.ContentId);
        var url = content == null ? "#" : content.Url(_publishedUrlProvider, redirectUrl.Culture);
        if (url.StartsWith("#"))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Route {Route} matches content {ContentId} which has no URL.", route, redirectUrl.ContentId);
            }

            return false;
        }

        // Appending any querystring from the incoming request to the redirect URL
        url = string.IsNullOrEmpty(frequest.Uri.Query) ? url : url + frequest.Uri.Query;
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Route {Route} matches content {ContentId} with URL '{Url}', redirecting.", route, content?.Id, url);
        }

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
}
