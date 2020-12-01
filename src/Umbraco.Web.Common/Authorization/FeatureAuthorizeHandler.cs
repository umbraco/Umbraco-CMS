using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Threading.Tasks;
using Umbraco.Web.Features;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Ensures that the controller is an authorized feature.
    /// </summary>
    public class FeatureAuthorizeHandler : AuthorizationHandler<FeatureAuthorizeRequirement>
    {
        private readonly UmbracoFeatures _umbracoFeatures;

        public FeatureAuthorizeHandler(UmbracoFeatures umbracoFeatures)
        {
            _umbracoFeatures = umbracoFeatures;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FeatureAuthorizeRequirement requirement)
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
            if (!(context.Resource is Endpoint endpoint))
            {
                throw new InvalidOperationException("This authorization handler can only be applied to controllers routed with endpoint routing");
            }

            var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            var controllerType = actionDescriptor.ControllerTypeInfo.AsType();
            return _umbracoFeatures.IsControllerEnabled(controllerType);
        }
    }
}
