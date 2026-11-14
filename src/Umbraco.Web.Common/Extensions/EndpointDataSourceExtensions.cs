using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Umbraco.Cms.Web.Common.Extensions;

/// <summary>
/// Extensions methods for EndpointDataSource.
/// </summary>
public static class EndpointDataSourceExtensions
{
    // Parsed route templates are cached by their raw text to avoid re-parsing the same fixed set of
    // endpoint patterns on every call. The key space is bounded by the number of registered endpoints.
    private static readonly ConcurrentDictionary<string, RouteTemplate> RouteTemplateCache = new();

    /// <summary>
    /// Gets an endpoint that matches the specified path.
    /// </summary>
    /// <param name="endpointDatasource">The end point data source.</param>
    /// <param name="linkParser">The link parsers using to parse the path.</param>
    /// <param name="path">The path to be parsed.</param>
    /// <param name="routeValues">The mathcing route values for the mathcing endpoint.</param>
    /// <returns>The endpoint or null if not found.</returns>
    /// <remarks>
    /// This only matches endpoints that have a route name, so it cannot find attribute-routed endpoints
    /// that were registered without a name. Use <see cref="GetEndpointByRoutePattern" /> to match those.
    /// </remarks>
    public static Endpoint? GetEndpointByPath(this EndpointDataSource endpointDatasource, LinkParser linkParser, PathString path, out RouteValueDictionary? routeValues)
    {
        routeValues = null;

        foreach (Endpoint endpoint in endpointDatasource.Endpoints)
        {
            routeValues = linkParser.ParsePathByEndpoint(endpoint, path);

            if (routeValues != null)
            {
                return endpoint;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the first endpoint whose route pattern matches the specified path and satisfies the predicate,
    /// matching by route pattern rather than route name (so unnamed, attribute-routed endpoints are found)
    /// and ignoring HTTP method constraints.
    /// </summary>
    /// <param name="endpointDataSource">The endpoint data source.</param>
    /// <param name="path">The path to match.</param>
    /// <param name="predicate">A predicate the matching endpoint must satisfy.</param>
    /// <param name="routeValues">The route values parsed from the matching endpoint, or null if none matched.</param>
    /// <returns>The matching endpoint, or null if none matched.</returns>
    public static Endpoint? GetEndpointByRoutePattern(
        this EndpointDataSource endpointDataSource,
        PathString path,
        Func<Endpoint, bool> predicate,
        out RouteValueDictionary? routeValues)
    {
        IEnumerable<RouteEndpoint> candidates = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => IsMatchableCandidate(endpoint, predicate))
            .OrderBy(endpoint => endpoint.Order);

        foreach (RouteEndpoint endpoint in candidates)
        {
            RouteTemplate routeTemplate = RouteTemplateCache.GetOrAdd(endpoint.RoutePattern.RawText!, TemplateParser.Parse);
            var matchedRouteValues = new RouteValueDictionary();
            if (new TemplateMatcher(routeTemplate, []).TryMatch(path, matchedRouteValues))
            {
                routeValues = matchedRouteValues;
                return endpoint;
            }
        }

        routeValues = null;
        return null;
    }

    /// <summary>
    /// Determines whether an endpoint is a matchable candidate: it can participate in routing and satisfies
    /// the caller's predicate.
    /// </summary>
    /// <remarks>
    /// An endpoint can participate in routing when it has a route pattern and is neither a dynamic endpoint
    /// (e.g. the Umbraco content catch-all, which matches every path) nor suppressed from matching.
    /// </remarks>
    private static bool IsMatchableCandidate(RouteEndpoint endpoint, Func<Endpoint, bool> predicate)
    {
        if (endpoint.RoutePattern.RawText is null)
        {
            return false;
        }

        if (endpoint.Metadata.OfType<IDynamicEndpointMetadata>().Any(metadata => metadata.IsDynamic))
        {
            return false;
        }

        if (endpoint.Metadata.OfType<ISuppressMatchingMetadata>().FirstOrDefault()?.SuppressMatching == true)
        {
            return false;
        }

        return predicate(endpoint);
    }
}
