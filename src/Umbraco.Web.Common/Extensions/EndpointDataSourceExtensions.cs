using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Umbraco.Cms.Web.Common.Extensions;

public static class EndpointDataSourceExtensions
{
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
