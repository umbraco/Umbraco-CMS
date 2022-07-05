using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Features;

namespace Umbraco.Cms.Web.Common.Authorization;

/// <summary>
///     Ensures that the controller is an authorized feature.
/// </summary>
public class FeatureAuthorizeHandler : AuthorizationHandler<FeatureAuthorizeRequirement>
{
    private readonly UmbracoFeatures _umbracoFeatures;

    public FeatureAuthorizeHandler(UmbracoFeatures umbracoFeatures) => _umbracoFeatures = umbracoFeatures;

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        FeatureAuthorizeRequirement requirement)
    {
        var allowed = IsAllowed(context);
        if (!allowed.HasValue || allowed.Value)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }

    private bool? IsAllowed(AuthorizationHandlerContext context)
    {
        Endpoint? endpoint = null;

        switch (context.Resource)
        {
            case DefaultHttpContext defaultHttpContext:
            {
                IEndpointFeature? endpointFeature = defaultHttpContext.Features.Get<IEndpointFeature>();
                endpoint = endpointFeature?.Endpoint;
                break;
            }

            case AuthorizationFilterContext authorizationFilterContext:
            {
                IEndpointFeature? endpointFeature =
                    authorizationFilterContext.HttpContext.Features.Get<IEndpointFeature>();
                endpoint = endpointFeature?.Endpoint;
                break;
            }

            case Endpoint resourceEndpoint:
            {
                endpoint = resourceEndpoint;
                break;
            }
        }

        if (endpoint is null)
        {
            throw new InvalidOperationException(
                "This authorization handler can only be applied to controllers routed with endpoint routing");
        }

        ControllerActionDescriptor? actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        Type? controllerType = actionDescriptor?.ControllerTypeInfo.AsType();
        return _umbracoFeatures.IsControllerEnabled(controllerType);
    }
}
