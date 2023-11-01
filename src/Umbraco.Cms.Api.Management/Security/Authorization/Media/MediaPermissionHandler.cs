using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <summary>
///     Authorizes that the current user has the correct permission access to the media item(s) specified in the request.
/// </summary>
public class MediaPermissionHandler : MustSatisfyRequirementAuthorizationHandler<MediaPermissionRequirement, IEnumerable<Guid>>
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
        IEnumerable<Guid> resource) =>
        await _mediaPermissionAuthorizer.IsAuthorizedAsync(context.User, resource);
}
