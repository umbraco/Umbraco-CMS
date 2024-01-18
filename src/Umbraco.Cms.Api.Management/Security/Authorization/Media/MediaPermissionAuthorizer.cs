using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <inheritdoc />
internal sealed class MediaPermissionAuthorizer : IMediaPermissionAuthorizer
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IMediaPermissionService _mediaPermissionService;

    public MediaPermissionAuthorizer(IAuthorizationHelper authorizationHelper, IMediaPermissionService mediaPermissionService)
    {
        _authorizationHelper = authorizationHelper;
        _mediaPermissionService = mediaPermissionService;
    }

    /// <inheritdoc />
    public async Task<bool> IsDeniedAsync(IPrincipal currentUser, IEnumerable<Guid> mediaKeys)
    {
        if (!mediaKeys.Any())
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _mediaPermissionService.AuthorizeAccessAsync(user, mediaKeys);

        // If we do not found it, we cannot tell if you are denied
        return result is not (MediaAuthorizationStatus.Success or MediaAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRootLevelAsync(IPrincipal currentUser)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _mediaPermissionService.AuthorizeRootAccessAsync(user);

        // If we do not found it, we cannot tell if you are denied
        return result is not (MediaAuthorizationStatus.Success or MediaAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRecycleBinLevelAsync(IPrincipal currentUser)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _mediaPermissionService.AuthorizeBinAccessAsync(user);

        // If we do not found it, we cannot tell if you are denied
        return result is not (MediaAuthorizationStatus.Success or MediaAuthorizationStatus.NotFound);
    }
}
