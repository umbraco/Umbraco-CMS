using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

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
    public async Task<bool> IsDeniedAsync(IPrincipal currentUser, IEnumerable<Guid> userKeys)
    {
        if (!userKeys.Any())
        {
            // We can't denied no keys.
            return false;
        }

        IUser performingUser = _authorizationHelper.GetUmbracoUser(currentUser);

        UserAuthorizationStatus result = await _userPermissionService.AuthorizeAccessAsync(performingUser, userKeys);

        return result is not UserAuthorizationStatus.Success;
    }
}
