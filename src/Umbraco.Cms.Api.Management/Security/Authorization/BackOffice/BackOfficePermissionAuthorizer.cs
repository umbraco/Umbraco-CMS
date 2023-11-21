using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.BackOffice;

/// <inheritdoc />
public class BackOfficePermissionAuthorizer : IBackOfficePermissionAuthorizer
{
    private readonly IRuntimeState _runtimeState;
    private readonly IAuthorizationHelper _authorizationHelper;

    public BackOfficePermissionAuthorizer(IRuntimeState runtimeState, IAuthorizationHelper authorizationHelper)
    {
        _runtimeState = runtimeState;
        _authorizationHelper = authorizationHelper;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAuthorizedAsync(IPrincipal currentUser, bool requireApproval)
    {
        // If not configured (during install or upgrade) we can continue
        // otherwise we need to ensure that a user is logged in
        switch (_runtimeState.Level)
        {
            case var _ when _runtimeState.EnableInstaller():
                return true;
            default:
                if (!currentUser.Identity?.IsAuthenticated ?? false)
                {
                    return false;
                }

                IUser user = _authorizationHelper.GetUmbracoUser(currentUser);
                var userApprovalSucceeded = !requireApproval || user.IsApproved;

                return userApprovalSucceeded;
        }
    }
}
