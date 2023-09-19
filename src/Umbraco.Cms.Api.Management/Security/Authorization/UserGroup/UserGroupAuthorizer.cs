using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;

/// <inheritdoc />
internal sealed class UserGroupAuthorizer : IUserGroupAuthorizer
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IUserGroupService _userGroupService;

    public UserGroupAuthorizer(IAuthorizationHelper authorizationHelper, IUserGroupService userGroupService)
    {
        _authorizationHelper = authorizationHelper;
        _userGroupService = userGroupService;
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> userGroupKeys)
    {
        if (!userGroupKeys.Any())
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        IUser? user = _authorizationHelper.GetCurrentUser(currentUser);

        var result = await _userGroupService.AuthorizeGroupAccessAsync(user, userGroupKeys);

        return result.Success;
    }
}
