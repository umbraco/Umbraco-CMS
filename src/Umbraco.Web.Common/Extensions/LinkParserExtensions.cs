using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Umbraco.Cms.Web.Common.Extensions;

public static class LinkParserExtensions
{
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
