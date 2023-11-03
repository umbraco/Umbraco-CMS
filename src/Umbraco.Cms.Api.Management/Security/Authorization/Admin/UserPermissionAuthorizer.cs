using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Admin;

/// <inheritdoc />
internal sealed class UserPermissionAuthorizer : IUserPermissionAuthorizer
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IUserPermissionService _userPermissionService;

    public UserPermissionAuthorizer(IAuthorizationHelper authorizationHelper, IUserPermissionService userPermissionService)
    {
        _authorizationHelper = authorizationHelper;
        _userPermissionService = userPermissionService;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> userKeys)
    {
        if (!userKeys.Any())
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        IUser performingUser = _authorizationHelper.GetCurrentUser(currentUser);

        var result = await _userPermissionService.AuthorizeAccessAsync(performingUser, userKeys);

        return result == UserAuthorizationStatus.Success;
    }
}
