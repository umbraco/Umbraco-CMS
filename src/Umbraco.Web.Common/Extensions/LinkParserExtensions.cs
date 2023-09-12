using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Umbraco.Cms.Web.Common.Extensions;

/// <summary>
/// Extension methods for LinkParser.
/// </summary>
public static class LinkParserExtensions
{
    /// <summary>
    /// Parses the path using the specified endpoint and returns the route values if a the path matches the route pattern.
    /// </summary>
    /// <param name="linkParser">The link parser.</param>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="path">The path to parse.</param>
    /// <returns>The route values if the path matches or null.</returns>
    public static RouteValueDictionary? ParsePathByEndpoint(this LinkParser linkParser, Endpoint endpoint, PathString path)
    {
        var name = endpoint.GetRouteName();

        if (name != null)
        {
            return linkParser.ParsePathByEndpointName(name, path);
        }
        else
        {
            return null;
        }
    }
}
