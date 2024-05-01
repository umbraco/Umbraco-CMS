using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Web.Website.Routing;


/**
 * A matcher policy that discards the catch-all (slug) route if there are any other valid routes with a lower order.
 *
 * The purpose of this is to skip our expensive <see cref="UmbracoRouteValueTransformer"/> if it's not required,
 * for instance if there's a statically routed endpoint registered before the dynamic route,
 * for more information see: https://github.com/umbraco/Umbraco-CMS/issues/16015.
 * The core reason why this is necessary is that ALL routes get evaluated:
 * "
 * all routes get evaluated, they get to produce candidates and then the best candidate is selected.
 * Since you have a dynamic route, it needs to run to produce the final endpoints and
 * then those are ranked in along with the rest of the candidates to choose the final endpoint.
 * "
 * From: https://github.com/dotnet/aspnetcore/issues/45175#issuecomment-1322497958
 *
 */

internal class EagerMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    // We want this to run as the very first policy, so we can discard the UmbracoRouteValueTransformer before the framework runs it.
    public override int Order => int.MinValue + 10;

    // We know we don't have to run this matcher against the backoffice endpoints.
    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
    {
        // If there's only one candidate it's the dynamic route
        if (endpoints.Count < 2)
        {
            return true;
        }

        foreach (Endpoint endpoint in endpoints)
        {
            ControllerActionDescriptor? actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            var namespaceName = actionDescriptor?.ControllerTypeInfo.Namespace;

            if (namespaceName is null || namespaceName.StartsWith("Umbraco.Cms") is false || namespaceName.StartsWith("Umbraco.Cms.Web.UI"))
            {
                continue;
            }

            return false;
        }

        return true;
    }

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
                // We have to ensure that the route is valid for the current request method.
                // This is because attribute routing will always have an order of 0.
                // This means that you could attribute route a POST to /example, but also have an umbraco page at /example
                // This would then result in a 404, because we'd see the attribute route with order 0, and always consider that the lowest order
                // We'd then disable the dynamic endpoint since another endpoint has a lower order, and end up with only 1 invalid endpoint.
                // (IsValidCandidate does not take this into account since the candidate itself is still valid)
                HttpMethodMetadata? methodMetaData = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
                if (methodMetaData?.HttpMethods.Contains(httpContext.Request.Method) is false)
                {
                    continue;
                }

                lowestOrder = routeEndpoint.Order;
            }

            if (routeEndpoint.DisplayName != Constants.Web.Routing.DynamicRoutePattern)
            {
                continue;
            }

            dynamicEndpoint = routeEndpoint;
            dynamicId = i;
        }

        // Invalidate the dynamic route if another route has a lower order.
        // This means that if you register your static route after the dynamic route, the dynamic route will take precedence
        // This more closely resembles the existing behaviour.
        if (dynamicEndpoint is not null && dynamicId is not null && dynamicEndpoint.Order > lowestOrder)
        {
            candidates.SetValidity(dynamicId.Value, false);
        }

        return Task.CompletedTask;
    }
}
