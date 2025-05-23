using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     Authorizes that the current user has the correct permission access to the content item(s) specified in the request.
/// </summary>
public class ContentPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ContentPermissionRequirement, ContentPermissionResource>
{
    private readonly IContentPermissionAuthorizer _contentPermissionAuthorizer;
    private readonly IAuthorizationHelper _authorizationHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionHandler" /> class.
    /// </summary>
    /// <param name="contentPermissionAuthorizer">Authorizer for content access.</param>
    /// <param name="authorizationHelper">The authorization helper.</param>
    public ContentPermissionHandler(IContentPermissionAuthorizer contentPermissionAuthorizer, IAuthorizationHelper authorizationHelper)
    {
        _contentPermissionAuthorizer = contentPermissionAuthorizer;
        _authorizationHelper = authorizationHelper;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ContentPermissionRequirement requirement,
        ContentPermissionResource resource)
    {
        var result = true;

        IUser user = _authorizationHelper.GetUmbracoUser(context.User);
        if (resource.CheckRoot)
        {
            result &= await _contentPermissionAuthorizer.IsDeniedAtRootLevelAsync(user, resource.PermissionsToCheck) is false;
        }

        if (resource.CheckRecycleBin)
        {
            result &= await _contentPermissionAuthorizer.IsDeniedAtRecycleBinLevelAsync(user, resource.PermissionsToCheck) is false;
        }

        if (resource.ParentKeyForBranch is not null)
        {
            result &= await _contentPermissionAuthorizer.IsDeniedWithDescendantsAsync(user, resource.ParentKeyForBranch.Value, resource.PermissionsToCheck) is false;
        }

        if (resource.ContentKeys.Any())
        {
            result &= await _contentPermissionAuthorizer.IsDeniedAsync(user, resource.ContentKeys, resource.PermissionsToCheck) is false;
        }

        if (resource.CulturesToCheck is not null)
        {
            result &= await _contentPermissionAuthorizer.IsDeniedForCultures(user, resource.CulturesToCheck) is false;
        }

        return result;
    }
}
