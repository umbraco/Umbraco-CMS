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
    public async Task<UserAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, IEnumerable<Guid> userKeys)
    {
        var currentIsAdmin = performingUser.IsAdmin();

        if (currentIsAdmin)
        {
            return UserAuthorizationStatus.Success;
        }

        var users = await _userService.GetAsync(userKeys);

        return users.Any(user => user.IsAdmin())
            ? UserAuthorizationStatus.UnauthorizedMissingAdminAccess
            : UserAuthorizationStatus.Success;
    }
}
