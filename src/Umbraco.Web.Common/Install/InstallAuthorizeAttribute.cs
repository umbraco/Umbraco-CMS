using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Umbraco.Web.Common.Install
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
            private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
            private readonly IRuntimeState _runtimeState;
            private readonly ILogger<InstallAuthorizeFilter> _logger;

            public InstallAuthorizeFilter(
                IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
                IRuntimeState runtimeState,
                ILogger<InstallAuthorizeFilter> logger)
            {
                _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
                _runtimeState = runtimeState;
                _logger = logger;
            }

            public void OnAuthorization(AuthorizationFilterContext authorizationFilterContext)
            {
                if (!IsAllowed())
                {
                    authorizationFilterContext.Result = new ForbidResult();
                }

            }

            private bool IsAllowed()
            {
                try
                {
                    // if not configured (install or upgrade) then we can continue
                    // otherwise we need to ensure that a user is logged in
                    return _runtimeState.Level == RuntimeLevel.Install
                           || _runtimeState.Level == RuntimeLevel.Upgrade
                           || (_backOfficeSecurityAccessor?.BackOfficeSecurity?.ValidateCurrentUser() ?? false);
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
