using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Contextual;

/// Authorizes a certain permission (read,write,browse,...) within a given context (umbraco, my-package,...)
/// against the <see cref="IGranularPermission">granular permissions</see> defined on all <see cref="IUserGroup">user groups</see> the <see cref="IUser">user</see> is part off.
/// Permissions and Context are checked on a InvariantCultureIgnoreCase basis.
public class ContextualPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ContextualPermissionRequirement,
    ContextualPermissionResource>
{
    public const string ContextualPermissionsPolicyAlias = "Umbraco.ContextualPermissions";
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IContextualPermissionAuthorizer _contextualPermissionAuthorizer;

    public ContextualPermissionHandler(
        IAuthorizationHelper authorizationHelper,
        IContextualPermissionAuthorizer contextualPermissionAuthorizer)
    {
        _authorizationHelper = authorizationHelper;
        _contextualPermissionAuthorizer = contextualPermissionAuthorizer;
    }

    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ContextualPermissionRequirement requirement,
        ContextualPermissionResource resource)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(context.User);

        return _contextualPermissionAuthorizer.IsDenied(user, resource) is false;
    }
}
