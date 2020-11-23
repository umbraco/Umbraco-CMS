using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Ensures authorization is successful for a back office user.
    /// </summary>
    public class BackOfficeHandler : AuthorizationHandler<BackOfficeRequirement>
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurity;
        private readonly IRuntimeState _runtimeState;

        public BackOfficeHandler(IBackOfficeSecurityAccessor backOfficeSecurity, IRuntimeState runtimeState)
        {
            _backOfficeSecurity = backOfficeSecurity;
            _runtimeState = runtimeState;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, BackOfficeRequirement requirement)
        {
            if (!IsAuthorized(requirement))
            {
                context.Fail();
            }
            else
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool IsAuthorized(BackOfficeRequirement requirement)
        {
            try
            {
                // if not configured (install or upgrade) then we can continue
                // otherwise we need to ensure that a user is logged in
                return _runtimeState.Level == RuntimeLevel.Install
                    || _runtimeState.Level == RuntimeLevel.Upgrade
                    || _backOfficeSecurity.BackOfficeSecurity?.ValidateCurrentUser(false, requirement.RequireApproval) == ValidateRequestAttempt.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
