using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class RequestRedirectService : RoutingServiceBase, IRequestRedirectService
{
    private readonly IRequestCultureService _requestCultureService;
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IApiPublishedContentCache _apiPublishedContentCache;
    private readonly IApiContentRouteBuilder _apiContentRouteBuilder;
    private readonly GlobalSettings _globalSettings;

    public RequestRedirectService(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IHttpContextAccessor httpContextAccessor,
        IRequestStartItemProviderAccessor requestStartItemProviderAccessor,
        IRequestCultureService requestCultureService,
        IRedirectUrlService redirectUrlService,
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentRouteBuilder apiContentRouteBuilder,
        IOptions<GlobalSettings> globalSettings)
        : base(publishedSnapshotAccessor, httpContextAccessor, requestStartItemProviderAccessor)
    {
        _requestCultureService = requestCultureService;
        _redirectUrlService = redirectUrlService;
        _apiPublishedContentCache = apiPublishedContentCache;
        _apiContentRouteBuilder = apiContentRouteBuilder;
        _globalSettings = globalSettings.Value;
    }

    public IApiContentRoute? GetRedirectRoute(string requestedPath)
    {
        requestedPath = requestedPath.EnsureStartsWith("/");

        IPublishedContent? startItem = GetStartItem();

        // must append the root content url segment if it is not hidden by config, because
        // the URL tracking is based on the actual URL, including the root content url segment
        if (_globalSettings.HideTopLevelNodeFromPath == false && startItem?.UrlSegment != null)
        {
            requestedPath = $"{startItem.UrlSegment.EnsureStartsWith("/")}{requestedPath}";
        }

        var culture = _requestCultureService.GetRequestedCulture();

        // important: redirect URLs are always tracked without trailing slashes
        requestedPath = requestedPath.TrimEnd("/");
        IRedirectUrl? redirectUrl = _redirectUrlService.GetMostRecentRedirectUrl(requestedPath, culture);

        // if a redirect URL was not found, try by appending the start item ID because URL tracking might have tracked
        // a redirect with "{root content ID}/{content path}"
        if (redirectUrl is null && startItem is not null)
        {
            redirectUrl = _redirectUrlService.GetMostRecentRedirectUrl($"{startItem.Id}{requestedPath}", culture);
        }

        // still no redirect URL found - try looking for a configured domain if we have a domain bound request,
        // because URL tracking might have tracked a redirect with "{domain content ID}/{content path}"
        if (redirectUrl is null)
        {
            Uri contentRoute = GetDefaultRequestUri(requestedPath);
            DomainAndUri? domainAndUri = GetDomainAndUriForRoute(contentRoute);
            if (domainAndUri is not null)
            {
                requestedPath = GetContentRoute(domainAndUri, contentRoute);
                culture ??= domainAndUri.Culture;
                redirectUrl = _redirectUrlService.GetMostRecentRedirectUrl(requestedPath, culture);
            }
        }

        IPublishedContent? content = redirectUrl != null
            ? _apiPublishedContentCache.GetById(redirectUrl.ContentKey)
            : null;

        return content != null
            ? _apiContentRouteBuilder.Build(content)
            : null;
    }
}
