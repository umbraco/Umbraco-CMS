using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class UserGroupPermissionService : IUserGroupPermissionService
{
    /// <inheritdoc/>
    public async Task<UserGroupAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, IEnumerable<Guid> userGroupKeys)
    {
        if (performingUser.IsAdmin())
        {
            return UserGroupAuthorizationStatus.Success;
        }

        var allowedUserGroupsKeys = performingUser.Groups.Select(x => x.Key).ToArray();
        var missingAccess = userGroupKeys.Except(allowedUserGroupsKeys).ToArray();

        return missingAccess.Length == 0
            ? UserGroupAuthorizationStatus.Success
            : UserGroupAuthorizationStatus.UnauthorizedMissingUserGroupAccess;
    }
}
