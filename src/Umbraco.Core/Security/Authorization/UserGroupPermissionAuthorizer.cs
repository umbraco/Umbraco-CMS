using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class UserGroupPermissionAuthorizer : IUserGroupPermissionAuthorizer
{
    private readonly IUserGroupPermissionService _userGroupPermissionService;

    public UserGroupPermissionAuthorizer(IUserGroupPermissionService userGroupPermissionService) =>
        _userGroupPermissionService = userGroupPermissionService;

    /// <inheritdoc />
    public async Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> userGroupKeys)
    {
        var userGroupKeysList = userGroupKeys.ToList();
        if (userGroupKeysList.Count == 0)
        {
            // We can't deny something that is not defined
            return false;
        }

        UserGroupAuthorizationStatus result =
            await _userGroupPermissionService.AuthorizeAccessAsync(currentUser, userGroupKeysList);

        return result is not UserGroupAuthorizationStatus.Success;
    }
}
