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
        for (var i = 0; i < candidates.Count; i++)
        {
            ControllerActionDescriptor? controllerActionDescriptor = candidates[i].Endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
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

    private static bool IsByIdsController(ControllerActionDescriptor? controllerActionDescriptor)
        => IsControllerType<ByIdsContentApiController>(controllerActionDescriptor) || IsControllerType<ByIdsMediaApiController>(controllerActionDescriptor);

    private static bool IsByRouteController(ControllerActionDescriptor? controllerActionDescriptor)
        => IsControllerType<ByRouteContentApiController>(controllerActionDescriptor) || IsControllerType<ByPathMediaApiController>(controllerActionDescriptor);

    private static bool IsControllerType<T>(ControllerActionDescriptor? controllerActionDescriptor)
        => controllerActionDescriptor?.MethodInfo.DeclaringType == typeof(T);
}
