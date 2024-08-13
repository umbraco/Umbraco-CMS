using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class UserPermissionAuthorizer : IUserPermissionAuthorizer
{
    private readonly IUserPermissionService _userPermissionService;

    public UserPermissionAuthorizer(IUserPermissionService userPermissionService) =>
        _userPermissionService = userPermissionService;

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> userKeys)
    {
        if (!userKeys.Any())
        {
            // We can't denied no keys.
            return false;
        }


        UserAuthorizationStatus result = await _userPermissionService.AuthorizeAccessAsync(currentUser, userKeys);

        return result is not UserAuthorizationStatus.Success;
    }
}
