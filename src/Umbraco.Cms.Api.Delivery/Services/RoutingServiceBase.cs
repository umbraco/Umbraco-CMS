using System.Web;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

internal abstract class RoutingServiceBase
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRequestStartItemProviderAccessor _requestStartItemProviderAccessor;

    protected RoutingServiceBase(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IHttpContextAccessor httpContextAccessor,
        IRequestStartItemProviderAccessor requestStartItemProviderAccessor)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _httpContextAccessor = httpContextAccessor;
        _requestStartItemProviderAccessor = requestStartItemProviderAccessor;
    }

    protected Uri GetDefaultRequestUri(string requestedPath)
    {
        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            throw new InvalidOperationException("Could not obtain an HTTP request context");
        }

        // construct the (assumed) absolute URL for the requested content
        return new Uri($"{request.Scheme}://{request.Host}{requestedPath}", UriKind.Absolute);
    }

    protected static string GetContentRoute(DomainAndUri domainAndUri, Uri contentRoute)
    {
        // Decoding the absolute path of contentRoute as PathRelativeToDomain needs to work with a decoded path value
        var decodedAbsolutePath = HttpUtility.UrlDecode(contentRoute.AbsolutePath);
        return $"{domainAndUri.ContentId}{DomainUtilities.PathRelativeToDomain(domainAndUri.Uri, decodedAbsolutePath)}";
    }

    protected DomainAndUri? GetDomainAndUriForRoute(Uri contentUrl)
    {
        IDomainCache? domainCache = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot().Domains;
        if (domainCache == null)
        {
            throw new InvalidOperationException("Could not obtain the domain cache in the current context");
        }

        IEnumerable<Domain> domains = domainCache.GetAll(false);

        return DomainUtilities.SelectDomain(domains, contentUrl, defaultCulture: domainCache.DefaultCulture);
    }

    protected IPublishedContent? GetStartItem()
    {
        if (_requestStartItemProviderAccessor.TryGetValue(out IRequestStartItemProvider? requestStartItemProvider) is false)
        {
            throw new InvalidOperationException($"Could not obtain an {nameof(IRequestStartItemProvider)} instance");
        }

        return requestStartItemProvider.GetStartItem();
    }
}
