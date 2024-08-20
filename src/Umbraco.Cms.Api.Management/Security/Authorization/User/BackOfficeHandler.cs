using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

/// <summary>
///     Ensures authorization is successful for a back office user.
/// </summary>
public class BackOfficeHandler : MustSatisfyRequirementAuthorizationHandler<BackOfficeRequirement>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurity;

    public BackOfficeHandler(IBackOfficeSecurityAccessor backOfficeSecurity)
    {
        _backOfficeSecurity = backOfficeSecurity;
    }

    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, BackOfficeRequirement requirement)
    {

        if (context.HasFailed is false && context.HasSucceeded is true)
        {
            return Task.FromResult(true);
        }

        if (!_backOfficeSecurity.BackOfficeSecurity?.IsAuthenticated() ?? false)
        {
            return Task.FromResult(false);
        }

        var userApprovalSucceeded = !requirement.RequireApproval ||
                                    (_backOfficeSecurity.BackOfficeSecurity?.CurrentUser?.IsApproved ?? false);
        return Task.FromResult(userApprovalSucceeded);
    }
}
