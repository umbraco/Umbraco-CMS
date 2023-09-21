using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <inheritdoc />
internal sealed class MediaAuthorizer : IMediaAuthorizer
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IMediaPermissionsService _mediaPermissionsService;

    public MediaAuthorizer(IAuthorizationHelper authorizationHelper, IMediaPermissionsService mediaPermissionsService)
    {
        _authorizationHelper = authorizationHelper;
        _mediaPermissionsService = mediaPermissionsService;
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> mediaKeys)
    {
        if (!mediaKeys.Any())
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        IUser user = _authorizationHelper.GetCurrentUser(currentUser);

        var result = await _mediaPermissionsService.AuthorizeAccessAsync(user, mediaKeys);

        return result == MediaAuthorizationStatus.Success;
    }
}
