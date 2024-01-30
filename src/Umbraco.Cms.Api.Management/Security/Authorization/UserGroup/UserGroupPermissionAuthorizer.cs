using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;

/// <inheritdoc />
internal sealed class UserGroupPermissionAuthorizer : IUserGroupPermissionAuthorizer
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IUserGroupPermissionService _userGroupPermissionService;

    public UserGroupPermissionAuthorizer(IAuthorizationHelper authorizationHelper, IUserGroupPermissionService userGroupPermissionService)
    {
        _authorizationHelper = authorizationHelper;
        _userGroupPermissionService = userGroupPermissionService;
    }

    /// <inheritdoc />
    public async Task<bool> IsDeniedAsync(IPrincipal currentUser, IEnumerable<Guid> userGroupKeys)
    {
        if (!userGroupKeys.Any())
        {
            // We can't deny something that is not defined
            return false;
        }

        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        UserGroupAuthorizationStatus result = await _userGroupPermissionService.AuthorizeAccessAsync(user, userGroupKeys);

        return result is not UserGroupAuthorizationStatus.Success;
    }
}
