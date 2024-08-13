using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <summary>
///     Authorizes that the current user has the correct permission access to the media item(s) specified in the request.
/// </summary>
public class MediaPermissionHandler : MustSatisfyRequirementAuthorizationHandler<MediaPermissionRequirement, MediaPermissionResource>
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IMediaPermissionAuthorizer _mediaPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPermissionHandler" /> class.
    /// </summary>
    /// <param name="mediaPermissionAuthorizer">Authorizer for media access.</param>
    /// <param name="authorizationHelper">The authorization helper.</param>
    public MediaPermissionHandler(IMediaPermissionAuthorizer mediaPermissionAuthorizer, IAuthorizationHelper authorizationHelper)
    {
        _mediaPermissionAuthorizer = mediaPermissionAuthorizer;
        _authorizationHelper = authorizationHelper;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        MediaPermissionRequirement requirement,
        MediaPermissionResource resource)
    {
        var result = true;

        IUser user = _authorizationHelper.GetUmbracoUser(context.User);

        if (resource.CheckRoot)
        {
            result &= await _mediaPermissionAuthorizer.IsDeniedAtRootLevelAsync(user) is false;
        }

        if (resource.CheckRecycleBin)
        {
            result &= await _mediaPermissionAuthorizer.IsDeniedAtRecycleBinLevelAsync(user) is false;
        }

        if (resource.MediaKeys.Any())
        {
            result &= await _mediaPermissionAuthorizer.IsDeniedAsync(user, resource.MediaKeys) is false;
        }

        return result;
    }
}
