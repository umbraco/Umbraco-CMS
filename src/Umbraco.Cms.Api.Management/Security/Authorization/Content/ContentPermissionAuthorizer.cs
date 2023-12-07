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
    public async Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> contentKeys, IReadOnlyList<char> permissionsToCheck)
    {
        if (!contentKeys.Any())
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _contentPermissionService.AuthorizeAccessAsync(user, contentKeys, permissionsToCheck);

        return result == ContentAuthorizationStatus.Success;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAuthorizedWithDescendantsAsync(IPrincipal currentUser, Guid parentKey, IReadOnlyList<char> permissionsToCheck)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _contentPermissionService.AuthorizeDescendantsAccessAsync(user, parentKey, permissionsToCheck);

        return result == ContentAuthorizationStatus.Success;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAuthorizedAtRootLevelAsync(IPrincipal currentUser, IReadOnlyList<char> permissionsToCheck)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _contentPermissionService.AuthorizeRootAccessAsync(user, permissionsToCheck);

        return result == ContentAuthorizationStatus.Success;
    }

    /// <inheritdoc/>
    public async Task<bool> IsAuthorizedAtRecycleBinLevelAsync(IPrincipal currentUser, IReadOnlyList<char> permissionsToCheck)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);

        var result = await _contentPermissionService.AuthorizeBinAccessAsync(user, permissionsToCheck);

        return result == ContentAuthorizationStatus.Success;
    }
}
