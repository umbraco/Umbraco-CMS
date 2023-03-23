using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Services;

public class RequestRoutingService : IRequestRoutingService
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRequestCultureService _requestCultureService;
    private readonly IRequestStartNodeService _requestStartNodeService;

    public RequestRoutingService(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IHttpContextAccessor httpContextAccessor,
        IRequestCultureService requestCultureService,
        IRequestStartNodeService requestStartNodeService)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _httpContextAccessor = httpContextAccessor;
        _requestCultureService = requestCultureService;
        _requestStartNodeService = requestStartNodeService;
    }

    public string GetContentRoute(string requestedPath)
    {
        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            throw new InvalidOperationException("Could not obtain an HTTP request context");
        }

        requestedPath = AppendRequestedStartNodePath(requestedPath);

        // construct the (assumed) absolute URL for the requested content, and use that
        // to look for a domain configuration that would match the URL
        var contentRoute = new Uri($"{request.Scheme}://{request.Host}{requestedPath}", UriKind.Absolute);
        DomainAndUri? domainAndUri = GetDomainAndUriForRoute(contentRoute);
        if (domainAndUri == null)
        {
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

    private string AppendRequestedStartNodePath(string requestedPath)
    {
        requestedPath = requestedPath.EnsureStartsWith("/");

        string? startNodePath = _requestStartNodeService.GetRequestedStartNodePath();

        return startNodePath.IsNullOrWhiteSpace()
            ? requestedPath
            : $"{startNodePath.EnsureStartsWith("/")}{requestedPath}";
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
