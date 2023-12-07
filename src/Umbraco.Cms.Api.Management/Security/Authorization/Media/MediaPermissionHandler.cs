using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <summary>
///     Authorizes that the current user has the correct permission access to the media item(s) specified in the request.
/// </summary>
public class MediaPermissionHandler : MustSatisfyRequirementAuthorizationHandler<MediaPermissionRequirement, MediaPermissionResource>
{
    private readonly IMediaPermissionAuthorizer _mediaPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPermissionHandler" /> class.
    /// </summary>
    /// <param name="mediaPermissionAuthorizer">Authorizer for media access.</param>
    public MediaPermissionHandler(IMediaPermissionAuthorizer mediaPermissionAuthorizer)
        => _mediaPermissionAuthorizer = mediaPermissionAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        MediaPermissionRequirement requirement,
        MediaPermissionResource resource)
    {
        if (resource.CheckRoot)
        {
            return await _mediaPermissionAuthorizer.IsAuthorizedAtRootLevelAsync(context.User);
        }

        if (resource.CheckRecycleBin)
        {
            return await _mediaPermissionAuthorizer.IsAuthorizedAtRecycleBinLevelAsync(context.User);
        }

        return await _mediaPermissionAuthorizer.IsAuthorizedAsync(context.User, resource.MediaKeys);
    }
}
