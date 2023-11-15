using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media.RecycleBin;

/// <summary>
///     Authorizes that the current user has access to the media recycle bin item.
/// </summary>
public class MediaRecycleBinPermissionHandler : MustSatisfyRequirementAuthorizationHandler<MediaRecycleBinPermissionRequirement>
{
    private readonly IMediaPermissionAuthorizer _mediaPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaRecycleBinPermissionHandler" /> class.
    /// </summary>
    /// <param name="mediaPermissionAuthorizer">Authorizer for media access.</param>
    public MediaRecycleBinPermissionHandler(IMediaPermissionAuthorizer mediaPermissionAuthorizer)
        => _mediaPermissionAuthorizer = mediaPermissionAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        MediaRecycleBinPermissionRequirement requirement) =>
        await _mediaPermissionAuthorizer.IsAuthorizedAtRecycleBinLevelAsync(context.User);
}
