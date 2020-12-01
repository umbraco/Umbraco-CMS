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
    public class BackOfficeHandler : MustSatisfyRequirementAuthorizationHandler<BackOfficeRequirement>
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurity;
        private readonly IRuntimeState _runtimeState;

        public BackOfficeHandler(IBackOfficeSecurityAccessor backOfficeSecurity, IRuntimeState runtimeState)
        {
            _backOfficeSecurity = backOfficeSecurity;
            _runtimeState = runtimeState;
        }

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, BackOfficeRequirement requirement)
        {
            try
            {
                // if not configured (install or upgrade) then we can continue
                // otherwise we need to ensure that a user is logged in
                var isAuth = _runtimeState.Level == RuntimeLevel.Install
                    || _runtimeState.Level == RuntimeLevel.Upgrade
                    || _backOfficeSecurity.BackOfficeSecurity?.ValidateCurrentUser(false, requirement.RequireApproval) == ValidateRequestAttempt.Success;
                return Task.FromResult(isAuth);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

    }
}
