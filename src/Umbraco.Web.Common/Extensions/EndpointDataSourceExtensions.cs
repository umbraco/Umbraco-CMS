using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Umbraco.Cms.Web.Common.Extensions;

/// <summary>
/// Extensions methods for EndpointDataSource.
/// </summary>
public static class EndpointDataSourceExtensions
{
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
        foreach (RouteEndpoint endpoint in endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .OrderBy(endpoint => endpoint.Order))
        {
            if (endpoint.RoutePattern.RawText is null)
            {
                continue;
            }

            // Skip dynamic endpoints (e.g. the Umbraco content catch-all) since they match every path.
            if (endpoint.Metadata.OfType<IDynamicEndpointMetadata>().Any(metadata => metadata.IsDynamic))
            {
                continue;
            }

            if (predicate(endpoint) is false)
            {
                continue;
            }

            var matchedRouteValues = new RouteValueDictionary();
            if (new TemplateMatcher(TemplateParser.Parse(endpoint.RoutePattern.RawText), []).TryMatch(path, matchedRouteValues))
            {
                routeValues = matchedRouteValues;
                return endpoint;
            }
        }

        routeValues = null;
        return null;
    }
}
