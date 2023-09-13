using System.Security.Claims;
using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;

/// <inheritdoc />
public class UserGroupAuthorizer : IUserGroupAuthorizer
{
    private readonly IUserService _userService;
    private readonly IUserGroupService _userGroupService;

    public UserGroupAuthorizer(IUserService userService, IUserGroupService userGroupService)
    {
        _userService = userService;
        _userGroupService = userGroupService;
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> userGroupKeys)
    {
        IUser? user = GetCurrentUser(currentUser);

        if (!userGroupKeys.Any())
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        var result = await _userGroupService.AuthorizeGroupAccessAsync(user, userGroupKeys);

        return result.Success;
    }

    private IUser? GetCurrentUser(IPrincipal? currentUser)
    {
        IUser? user = null;
        ClaimsIdentity? umbIdentity = currentUser?.GetUmbracoIdentity();
        Guid? currentUserKey = umbIdentity?.GetUserKey();

        if (currentUserKey is null)
        {
            int? currentUserId = umbIdentity?.GetUserId<int>();
            if (currentUserId.HasValue)
            {
                user = _userService.GetUserById(currentUserId.Value);
            }
        }
        else
        {
            user = _userService.GetAsync(currentUserKey.Value).GetAwaiter().GetResult();
        }

        return user;
    }
}
