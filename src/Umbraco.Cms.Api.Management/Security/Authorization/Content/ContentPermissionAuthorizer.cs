using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <inheritdoc />
internal sealed class ContentPermissionAuthorizer : IContentPermissionAuthorizer
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IContentPermissionService _contentPermissionService;

    public ContentPermissionAuthorizer(IAuthorizationHelper authorizationHelper, IContentPermissionService contentPermissionService)
    {
        _authorizationHelper = authorizationHelper;
        _contentPermissionService = contentPermissionService;
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAsync(IPrincipal currentUser, IEnumerable<Guid> contentKeys, ISet<char> permissionsToCheck)
    {
        if (!contentKeys.Any())
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _contentPermissionService.AuthorizeAccessAsync(user, contentKeys, permissionsToCheck);

        // If we do not found it, we cannot tell if you are denied
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedWithDescendantsAsync(IPrincipal currentUser, Guid parentKey, ISet<char> permissionsToCheck)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _contentPermissionService.AuthorizeDescendantsAccessAsync(user, parentKey, permissionsToCheck);

        // If we do not found it, we cannot tell if you are denied
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRootLevelAsync(IPrincipal currentUser, ISet<char> permissionsToCheck)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _contentPermissionService.AuthorizeRootAccessAsync(user, permissionsToCheck);

        // If we do not found it, we cannot tell if you are denied
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRecycleBinLevelAsync(IPrincipal currentUser, ISet<char> permissionsToCheck)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _contentPermissionService.AuthorizeBinAccessAsync(user, permissionsToCheck);

        // If we do not found it, we cannot tell if you are denied
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }

    public async Task<bool> IsDeniedForCultures(IPrincipal currentUser, ISet<string> culturesToCheck)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);
        ContentAuthorizationStatus result = await _contentPermissionService.AuthorizeCultureAccessAsync(user, culturesToCheck);
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }
}
