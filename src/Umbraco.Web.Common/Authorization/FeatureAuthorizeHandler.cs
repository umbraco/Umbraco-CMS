using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Web.Common.Authorization;

/// <summary>
///     Ensures that the controller is an authorized feature.
/// </summary>
public class FeatureAuthorizeHandler : AuthorizationHandler<FeatureAuthorizeRequirement>
{
    private readonly UmbracoFeatures _umbracoFeatures;
    private readonly IRuntimeState _runtimeState;

    public FeatureAuthorizeHandler(UmbracoFeatures umbracoFeatures, IRuntimeState runtimeState)
    {
        _umbracoFeatures = umbracoFeatures;
        _runtimeState = runtimeState;
    }

    [Obsolete("Use ctor that is not obsolete. This will be removed in v13.")]
    public FeatureAuthorizeHandler(UmbracoFeatures umbracoFeatures)
        :this(umbracoFeatures, StaticServiceProvider.Instance.GetRequiredService<IRuntimeState>())
    {

    }

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
        if(_runtimeState.Level != RuntimeLevel.Run && _runtimeState.Level != RuntimeLevel.Upgrade)
        {
            return false;
        }

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
