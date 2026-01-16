using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Element;

/// <summary>
///     Authorizes that the current user has the correct permission access to the element item(s) specified in the request.
/// </summary>
public class ElementPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ElementPermissionRequirement, ElementPermissionResource>
{
    private readonly IElementPermissionAuthorizer _elementPermissionAuthorizer;
    private readonly IAuthorizationHelper _authorizationHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementPermissionHandler" /> class.
    /// </summary>
    /// <param name="elementPermissionAuthorizer">Authorizer for element access.</param>
    /// <param name="authorizationHelper">The authorization helper.</param>
    public ElementPermissionHandler(IElementPermissionAuthorizer elementPermissionAuthorizer, IAuthorizationHelper authorizationHelper)
    {
        _elementPermissionAuthorizer = elementPermissionAuthorizer;
        _authorizationHelper = authorizationHelper;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ElementPermissionRequirement requirement,
        ElementPermissionResource resource)
    {
        var result = true;

        IUser user = _authorizationHelper.GetUmbracoUser(context.User);
        if (resource.CheckRoot)
        {
            result &= await _elementPermissionAuthorizer.IsDeniedAtRootLevelAsync(user, resource.PermissionsToCheck) is false;
        }

        if (resource.CheckRecycleBin)
        {
            result &= await _elementPermissionAuthorizer.IsDeniedAtRecycleBinLevelAsync(user, resource.PermissionsToCheck) is false;
        }

        if (resource.ParentKeyForBranch is not null)
        {
            result &= await _elementPermissionAuthorizer.IsDeniedWithDescendantsAsync(user, resource.ParentKeyForBranch.Value, resource.PermissionsToCheck) is false;
        }

        if (resource.ElementKeys.Any())
        {
            result &= await _elementPermissionAuthorizer.IsDeniedAsync(user, resource.ElementKeys, resource.PermissionsToCheck) is false;
        }

        if (resource.CulturesToCheck is not null)
        {
            result &= await _elementPermissionAuthorizer.IsDeniedForCultures(user, resource.CulturesToCheck) is false;
        }

        return result;
    }
}