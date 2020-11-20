using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Core;

namespace Umbraco.Web.Common.Install
{
    /// <summary>
    /// Ensures authorization occurs for the installer if it has already completed.
    /// If install has not yet occurred then the authorization is successful.
    /// </summary>
    public class InstallAuthorizeAttribute : TypeFilterAttribute
    {
        // NOTE: This doesn't need to be an authz policy, it's only used for the installer

        public InstallAuthorizeAttribute() : base(typeof(InstallAuthorizeFilter))
        {
        }

        private class InstallAuthorizeFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext authorizationFilterContext)
            {
                var serviceProvider = authorizationFilterContext.HttpContext.RequestServices;
                var runtimeState = serviceProvider.GetService<IRuntimeState>();
                var umbracoContext = serviceProvider.GetService<IUmbracoContext>();
                var logger = serviceProvider.GetService<ILogger<InstallAuthorizeFilter>>();

                if (!IsAllowed(runtimeState, umbracoContext, logger))
                {
                    authorizationFilterContext.Result = new ForbidResult();
                }

            }

            private static bool IsAllowed(IRuntimeState runtimeState, IUmbracoContext umbracoContext, ILogger<InstallAuthorizeFilter> logger)
            {
                try
                {
                    // if not configured (install or upgrade) then we can continue
                    // otherwise we need to ensure that a user is logged in
                    return runtimeState.Level == RuntimeLevel.Install
                           || runtimeState.Level == RuntimeLevel.Upgrade
                           || (umbracoContext?.Security?.ValidateCurrentUser() ?? false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred determining authorization");
                    return false;
                }
            }
        }
    }

}
