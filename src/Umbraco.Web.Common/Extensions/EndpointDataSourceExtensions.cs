using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
}
