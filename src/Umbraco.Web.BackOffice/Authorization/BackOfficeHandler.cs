// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
            // if not configured (install or upgrade) then we can continue
            // otherwise we need to ensure that a user is logged in

            switch (_runtimeState.Level)
            {
                case RuntimeLevel.Install:
                case RuntimeLevel.Upgrade:
                    return Task.FromResult(true);
                default:
                    if (!_backOfficeSecurity.BackOfficeSecurity.IsAuthenticated())
                    {
                        return Task.FromResult(false);
                    }

                    var userApprovalSucceeded = !requirement.RequireApproval || (_backOfficeSecurity.BackOfficeSecurity.CurrentUser?.IsApproved ?? false);
                    return Task.FromResult(userApprovalSucceeded);
            }
        }

    }
}
