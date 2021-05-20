using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Install
{
    /// <summary>
    /// Ensures authorization occurs for the installer if it has already completed.
    /// If install has not yet occurred then the authorization is successful.
    /// </summary>
    public class InstallAuthorizeAttribute : TypeFilterAttribute
    {
        public InstallAuthorizeAttribute() : base(typeof(InstallAuthorizeFilter))
        {
        }

        private class InstallAuthorizeFilter : IAuthorizationFilter
        {
            private readonly IRuntimeState _runtimeState;
            private readonly ILogger<InstallAuthorizeFilter> _logger;

            public InstallAuthorizeFilter(
                IRuntimeState runtimeState,
                ILogger<InstallAuthorizeFilter> logger)
            {
                _runtimeState = runtimeState;
                _logger = logger;
            }

            public void OnAuthorization(AuthorizationFilterContext authorizationFilterContext)
            {
                if (!IsAllowed(authorizationFilterContext))
                {
                    authorizationFilterContext.Result = new ForbidResult();
                }
            }

            private bool IsAllowed(AuthorizationFilterContext authorizationFilterContext)
            {
                try
                {
                    // if not configured (install or upgrade) then we can continue
                    // otherwise we need to ensure that a user is logged in
                    return _runtimeState.Level == RuntimeLevel.Install
                           || _runtimeState.Level == RuntimeLevel.Upgrade
                           || (authorizationFilterContext.HttpContext.User?.Identity?.IsAuthenticated ?? false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred determining authorization");
                    return false;
                }
            }
        }
    }

}
