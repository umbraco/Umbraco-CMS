using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Services;

internal sealed class RequestRoutingService : IRequestRoutingService
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRequestCultureService _requestCultureService;
    private readonly IRequestStartItemProviderAccessor _requestStartItemProviderAccessor;

    public RequestRoutingService(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IHttpContextAccessor httpContextAccessor,
        IRequestCultureService requestCultureService,
        IRequestStartItemProviderAccessor requestStartItemProviderAccessor)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _httpContextAccessor = httpContextAccessor;
        _requestCultureService = requestCultureService;
        _requestStartItemProviderAccessor = requestStartItemProviderAccessor;
    }

    /// <inheritdoc />
    public string GetContentRoute(string requestedPath)
    {
        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            throw new InvalidOperationException("Could not obtain an HTTP request context");
        }

        if (_requestStartItemProviderAccessor.TryGetValue(out IRequestStartItemProvider? requestStartItemProvider) is false)
        {
            throw new InvalidOperationException($"Could not obtain an {nameof(IRequestStartItemProvider)} instance");
        }

        requestedPath = requestedPath.EnsureStartsWith("/");

        // do we have an explicit start item?
        IPublishedContent? startItem = requestStartItemProvider.GetStartItem();
        if (startItem != null)
        {
            // the content cache can resolve content by the route "{root ID}/{content path}", which is what we construct here
            return $"{startItem.Id}{requestedPath}";
        }

        // construct the (assumed) absolute URL for the requested content, and use that
        // to look for a domain configuration that would match the URL
        var contentRoute = new Uri($"{request.Scheme}://{request.Host}{requestedPath}", UriKind.Absolute);
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
        return $"{domainAndUri.ContentId}{DomainUtilities.PathRelativeToDomain(domainAndUri.Uri, contentRoute.AbsolutePath)}";
    }

    private DomainAndUri? GetDomainAndUriForRoute(Uri contentUrl)
    {
        IDomainCache? domainCache = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Domains;
        if (domainCache == null)
        {
            throw new InvalidOperationException("Could not obtain the domain cache in the current context");
        }

        IEnumerable<Domain> domains = domainCache.GetAll(false);

        return DomainUtilities.SelectDomain(domains, contentUrl, defaultCulture: domainCache.DefaultCulture);
    }
}
