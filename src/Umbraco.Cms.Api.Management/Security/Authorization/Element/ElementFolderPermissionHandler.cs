using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Element;

/// <summary>
///     Authorizes that the current user has the correct permission access to the element folder item(s) specified in the request.
/// </summary>
public class ElementFolderPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ElementFolderPermissionRequirement, ElementFolderPermissionResource>
{
    private readonly IElementFolderPermissionAuthorizer _elementFolderPermissionAuthorizer;
    private readonly IAuthorizationHelper _authorizationHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementFolderPermissionHandler" /> class.
    /// </summary>
    /// <param name="elementFolderPermissionAuthorizer">Authorizer for element folder access.</param>
    /// <param name="authorizationHelper">The authorization helper.</param>
    public ElementFolderPermissionHandler(IElementFolderPermissionAuthorizer elementFolderPermissionAuthorizer, IAuthorizationHelper authorizationHelper)
    {
        _elementFolderPermissionAuthorizer = elementFolderPermissionAuthorizer;
        _authorizationHelper = authorizationHelper;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ElementFolderPermissionRequirement requirement,
        ElementFolderPermissionResource resource)
    {
        var result = true;

        IUser user = _authorizationHelper.GetUmbracoUser(context.User);
        if (resource.CheckRoot)
        {
            result &= await _elementFolderPermissionAuthorizer.IsDeniedAtRootLevelAsync(user, resource.PermissionsToCheck) is false;
        }

        if (resource.CheckRecycleBin)
        {
            result &= await _elementFolderPermissionAuthorizer.IsDeniedAtRecycleBinLevelAsync(user, resource.PermissionsToCheck) is false;
        }

        if (resource.FolderKeys.Any())
        {
            result &= await _elementFolderPermissionAuthorizer.IsDeniedAsync(user, resource.FolderKeys, resource.PermissionsToCheck) is false;
        }

        return result;
    }
}
