using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Web.Website.Routing;

public class EagerMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    // We want this to run as the very first policy, so we can discard the UmbracoRouteValueTransformer before the framework runs it.
    public override int Order => int.MinValue + 10;

    // We want to run this against all endpoints.
    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints) => true;

    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        // If there's only one candidate, we don't need to do anything.
        if (candidates.Count < 2)
        {
            return Task.CompletedTask;
        }

        // If there are multiple candidates, we want to discard the catch-all (slug)
        // IF there is any candidates with a lower order. Since this will be a statically routed endpoint registered before the dynamic route.
        // Which means that we don't have to run our UmbracoRouteValueTransformer to route dynamically (expensive).
        var lowestOrder = int.MaxValue;
        int? dynamicId = null;
        RouteEndpoint? dynamicEndpoint = null;
        for (var i = 0; i < candidates.Count; i++)
        {
            CandidateState candidate = candidates[i];

            // If it's not a RouteEndpoint there's not much we can do to count it in the order.
            if (candidate.Endpoint is not RouteEndpoint routeEndpoint)
            {
                continue;
            }

            if (routeEndpoint.Order < lowestOrder)
            {
                lowestOrder = routeEndpoint.Order;
            }

            if (routeEndpoint.DisplayName != Constants.Web.Routing.DynamicRoutePattern)
            {
                continue;
            }

            dynamicEndpoint = routeEndpoint;
            dynamicId = i;
        }

        // Invalidate the dynamic route if a static route has a lower order.
        if (dynamicEndpoint is not null && dynamicId is not null && dynamicEndpoint.Order > lowestOrder)
        {
            candidates.SetValidity(dynamicId.Value, false);
        }

        return Task.CompletedTask;
    }
}
