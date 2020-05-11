
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Web.Features;
using Umbraco.Core;
using Umbraco.Web.Install;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Ensures that the controller is an authorized feature.
    /// </summary>
    /// <remarks>Else returns unauthorized.</remarks>
    public class FeatureAuthorizeAttribute : TypeFilterAttribute
    {
        public FeatureAuthorizeAttribute() : base(typeof(FeatureAuthorizeFilter))
        {
        }

        private class FeatureAuthorizeFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var serviceProvider = context.HttpContext.RequestServices;
                var umbracoFeatures = serviceProvider.GetService<UmbracoFeatures>();

                if (!IsAllowed(context, umbracoFeatures))
                {
                    context.Result = new ForbidResult();
                }
            }

            private static bool IsAllowed(AuthorizationFilterContext context, UmbracoFeatures umbracoFeatures)
            {
                // if no features resolver has been set then return true, this will occur in unit
                // tests and we don't want users to have to set a resolver
                //just so their unit tests work.

                if (umbracoFeatures == null) return true;
                if (!(context.ActionDescriptor is ControllerActionDescriptor contextActionDescriptor)) return true;

                var controllerType = contextActionDescriptor.ControllerTypeInfo.AsType();
                return umbracoFeatures.IsControllerEnabled(controllerType);
            }
        }
    }
}
