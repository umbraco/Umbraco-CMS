using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Umbraco.Cms.Api.Delivery.Controllers.Content;
using Umbraco.Cms.Api.Delivery.Controllers.Media;

namespace Umbraco.Cms.Api.Delivery.Routing;

// FIXME: remove this when Delivery API V1 is removed
internal sealed class DeliveryApiItemsEndpointsMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    public override int Order => 100;

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
    {
        for (var i = 0; i < endpoints.Count; i++)
        {
            ControllerActionDescriptor? controllerActionDescriptor = endpoints[i].Metadata.GetMetadata<ControllerActionDescriptor>();
            if (IsByIdsController(controllerActionDescriptor) || IsByRouteController(controllerActionDescriptor))
            {
                return true;
            }
        }

        return false;
    }

    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        var hasIdQueryParameter = httpContext.Request.Query.ContainsKey("id");
        ApiVersion? requestedApiVersion = httpContext.GetRequestedApiVersion();
        for (var i = 0; i < candidates.Count; i++)
        {
            CandidateState candidate = candidates[i];
            Endpoint? endpoint = candidate.Endpoint;

            // NOTE: nullability for the CandidateState.Endpoint property is not correct - it *can* be null
            if (endpoint is null)
            {
                continue;
            }

            if (EndpointSupportsApiVersion(endpoint, requestedApiVersion) is false)
            {
                candidates.SetValidity(i, false);
                continue;
            }

            ControllerActionDescriptor? controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (IsByIdsController(controllerActionDescriptor))
            {
                candidates.SetValidity(i, hasIdQueryParameter);
            }
            else if (IsByRouteController(controllerActionDescriptor))
            {
                candidates.SetValidity(i, hasIdQueryParameter is false);
            }
        }

        return Task.CompletedTask;
    }

    private static bool EndpointSupportsApiVersion(Endpoint endpoint, ApiVersion? requestedApiVersion)
    {
        ApiVersion[]? supportedApiVersions = endpoint.Metadata.GetMetadata<MapToApiVersionAttribute>()?.Versions.ToArray();

        // if the endpoint is versioned, the requested API version must be among the API versions supported by the endpoint.
        // if the endpoint is NOT versioned, it cannot be used with a requested API version
        return supportedApiVersions?.Contains(requestedApiVersion) ?? requestedApiVersion is null;
    }

    private static bool IsByIdsController(ControllerActionDescriptor? controllerActionDescriptor)
        => IsControllerType<ByIdsContentApiController>(controllerActionDescriptor) || IsControllerType<ByIdsMediaApiController>(controllerActionDescriptor);

    private static bool IsByRouteController(ControllerActionDescriptor? controllerActionDescriptor)
        => IsControllerType<ByRouteContentApiController>(controllerActionDescriptor) || IsControllerType<ByPathMediaApiController>(controllerActionDescriptor);

    private static bool IsControllerType<T>(ControllerActionDescriptor? controllerActionDescriptor)
        => controllerActionDescriptor?.MethodInfo.DeclaringType == typeof(T);
}
