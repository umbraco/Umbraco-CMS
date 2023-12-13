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
    public async Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> mediaKeys)
    {
        if (!mediaKeys.Any())
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _mediaPermissionService.AuthorizeAccessAsync(user, mediaKeys);

        return result == MediaAuthorizationStatus.Success;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAuthorizedAtRootLevelAsync(IPrincipal currentUser)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _mediaPermissionService.AuthorizeRootAccessAsync(user);

        return result == MediaAuthorizationStatus.Success;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAuthorizedAtRecycleBinLevelAsync(IPrincipal currentUser)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _mediaPermissionService.AuthorizeBinAccessAsync(user);

        return result == MediaAuthorizationStatus.Success;
    }
}
