using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Element;

/// <summary>
///     Authorizes that the current user has the correct permission access to the element container item(s) specified in the request.
/// </summary>
public class ElementContainerPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ElementcontainerPermissionRequirement, ElementContainerPermissionResource>
{
    private readonly IElementContainerPermissionAuthorizer _elementContainerPermissionAuthorizer;
    private readonly IAuthorizationHelper _authorizationHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementContainerPermissionHandler" /> class.
    /// </summary>
    /// <param name="elementContainerPermissionAuthorizer">Authorizer for element container access.</param>
    /// <param name="authorizationHelper">The authorization helper.</param>
    public ElementContainerPermissionHandler(IElementContainerPermissionAuthorizer elementContainerPermissionAuthorizer, IAuthorizationHelper authorizationHelper)
    {
        _elementContainerPermissionAuthorizer = elementContainerPermissionAuthorizer;
        _authorizationHelper = authorizationHelper;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ElementcontainerPermissionRequirement requirement,
        ElementContainerPermissionResource resource)
    {
        var result = true;

        IUser user = _authorizationHelper.GetUmbracoUser(context.User);
        if (resource.CheckRoot)
        {
            result &= await _elementContainerPermissionAuthorizer.IsDeniedAtRootLevelAsync(user, resource.PermissionsToCheck) is false;
        }

        if (resource.CheckRecycleBin)
        {
            result &= await _elementContainerPermissionAuthorizer.IsDeniedAtRecycleBinLevelAsync(user, resource.PermissionsToCheck) is false;
        }

        if (resource.ContainerKeys.Any())
        {
            result &= await _elementContainerPermissionAuthorizer.IsDeniedAsync(user, resource.ContainerKeys, resource.PermissionsToCheck) is false;
        }

        return result;
    }
}
