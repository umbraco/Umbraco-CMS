using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media.Root;

/// <summary>
///     Authorizes that the current user has access to the media root item.
/// </summary>
public class MediaRootPermissionHandler : MustSatisfyRequirementAuthorizationHandler<MediaRootPermissionRequirement>
{
    private readonly IMediaPermissionAuthorizer _mediaPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaRootPermissionHandler" /> class.
    /// </summary>
    /// <param name="mediaPermissionAuthorizer">Authorizer for media access.</param>
    public MediaRootPermissionHandler(IMediaPermissionAuthorizer mediaPermissionAuthorizer)
        => _mediaPermissionAuthorizer = mediaPermissionAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        MediaRootPermissionRequirement requirement) =>
        await _mediaPermissionAuthorizer.IsAuthorizedAtRootLevelAsync(context.User);
}
