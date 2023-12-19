using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Authorization;

/// <summary>
///     Ensures authorization have permission to create content blueprints.
/// </summary>
public class ContentBlueprintHandler : MustSatisfyRequirementAuthorizationHandler<ContentBlueprintRequirement>
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurity;
    private readonly IUserService _userService;

    public ContentBlueprintHandler(IBackOfficeSecurityAccessor backOfficeSecurity, IUserService userService)
    {
        _backOfficeSecurity = backOfficeSecurity;
        _userService = userService;
    }

    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, ContentBlueprintRequirement requirement)
    {
        IUser? user = _backOfficeSecurity.BackOfficeSecurity?.CurrentUser;
        if (user is null)
        {
            return Task.FromResult(false);
        }

        // Check if a given user group has permission to create a content template
        foreach (IReadOnlyUserGroup userGroup in user.Groups)
        {
            if (userGroup.Permissions is null)
            {
                continue;
            }

            if (userGroup.Permissions.Any(x => x.Contains(ActionCreateBlueprintFromContent.ActionLetter)))
            {
                return Task.FromResult(true);
            }
        }

        // If the user groups did not have permissions, we have to also
        // check granular permissions, as you could potentially have permission there.
        EntityPermissionCollection permissions = _userService.GetPermissions(user);
        return Task.FromResult(permissions.GetAllPermissions().Any(x => x.Contains(ActionCreateBlueprintFromContent.ActionLetter)));
    }
}
