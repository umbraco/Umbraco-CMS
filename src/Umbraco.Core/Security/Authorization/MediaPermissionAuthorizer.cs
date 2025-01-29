using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class MediaPermissionAuthorizer : IMediaPermissionAuthorizer
{
    private readonly IMediaPermissionService _mediaPermissionService;

    public MediaPermissionAuthorizer(IMediaPermissionService mediaPermissionService) =>
        _mediaPermissionService = mediaPermissionService;

    /// <inheritdoc />
    public async Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> mediaKeys)
    {
        if (!mediaKeys.Any())
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        MediaAuthorizationStatus result = await _mediaPermissionService.AuthorizeAccessAsync(currentUser, mediaKeys);

        // If we can't find the media item(s) then we can't determine whether you are denied access.
        return result is not (MediaAuthorizationStatus.Success or MediaAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser)
    {
        MediaAuthorizationStatus result = await _mediaPermissionService.AuthorizeRootAccessAsync(currentUser);

        // If we can't find the media item(s) then we can't determine whether you are denied access.
        return result is not (MediaAuthorizationStatus.Success or MediaAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser)
    {
        MediaAuthorizationStatus result = await _mediaPermissionService.AuthorizeBinAccessAsync(currentUser);

        // If we can't find the media item(s) then we can't determine whether you are denied access.
        return result is not (MediaAuthorizationStatus.Success or MediaAuthorizationStatus.NotFound);
    }
}
