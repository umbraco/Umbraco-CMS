﻿using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class RequestRoutingService : RoutingServiceBase, IRequestRoutingService
{
    private readonly IRequestCultureService _requestCultureService;

    public RequestRoutingService(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IHttpContextAccessor httpContextAccessor,
        IRequestStartItemProviderAccessor requestStartItemProviderAccessor,
        IRequestCultureService requestCultureService)
        : base(publishedSnapshotAccessor, httpContextAccessor, requestStartItemProviderAccessor) =>
        _requestCultureService = requestCultureService;

    /// <inheritdoc />
    public string GetContentRoute(string requestedPath)
    {
        if (requestedPath.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        requestedPath = requestedPath.EnsureStartsWith("/");

        // do we have an explicit start item?
        IPublishedContent? startItem = GetStartItem();
        if (startItem != null)
        {
            // the content cache can resolve content by the route "{root ID}/{content path}", which is what we construct here
            return $"{startItem.Id}{requestedPath}";
        }

        // construct the (assumed) absolute URL for the requested content, and use that
        // to look for a domain configuration that would match the URL
        Uri contentRoute = GetDefaultRequestUri(requestedPath);
        DomainAndUri? domainAndUri = GetDomainAndUriForRoute(contentRoute);
        if (domainAndUri == null)
        {
            // no start item was found and no domain could be resolved, we will return the requested path
            // as route and hope the content cache can resolve that (it likely can)
            return requestedPath;
        }

        // the Accept-Language header takes precedence over configured domain culture
        if (domainAndUri.Culture != null && _requestCultureService.GetRequestedCulture().IsNullOrWhiteSpace())
        {
            _requestCultureService.SetRequestCulture(domainAndUri.Culture);
        }

        // when resolving content from a configured domain, the content cache expects the content route
        // to be "{domain content ID}/{content path}", which is what we construct here
        return GetContentRoute(domainAndUri, contentRoute);
    }
}
