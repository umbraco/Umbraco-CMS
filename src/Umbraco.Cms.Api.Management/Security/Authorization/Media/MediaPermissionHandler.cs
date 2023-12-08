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
        var result = true;

        if (resource.CheckRoot)
        {
            result &= await _mediaPermissionAuthorizer.IsAuthorizedAtRootLevelAsync(context.User);
        }

        if (resource.CheckRecycleBin)
        {
            result &= await _mediaPermissionAuthorizer.IsAuthorizedAtRecycleBinLevelAsync(context.User);
        }

        if (resource.MediaKeys.Any())
        {
            result &= await _mediaPermissionAuthorizer.IsAuthorizedAsync(context.User, resource.MediaKeys);
        }

        return result;
    }
}
