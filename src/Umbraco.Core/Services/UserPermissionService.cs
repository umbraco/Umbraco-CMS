using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class UserPermissionService : IUserPermissionService
{
    private readonly IUserService _userService;

    public UserPermissionService(IUserService userService)
        => _userService = userService;

    /// <inheritdoc/>
    public async Task<UserAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> userKeys)
    {
        var currentIsAdmin = user.IsAdmin();

        if (currentIsAdmin)
        {
            return UserAuthorizationStatus.Success;
        }

        var usersToCheck = await _userService.GetAsync(userKeys);

        return usersToCheck.Any(u => u.IsAdmin())
            ? UserAuthorizationStatus.UnauthorizedMissingAdminAccess
            : UserAuthorizationStatus.Success;
    }
}
