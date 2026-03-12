using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

/// <summary>
///     Authorizes that the current user has the correct permission access to the applications listed in the requirement.
/// </summary>
internal sealed class AllowedApplicationHandler : MustSatisfyRequirementAuthorizationHandler<AllowedApplicationRequirement>
{
    private readonly IAuthorizationHelper _authorizationHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedApplicationHandler"/> class.
    /// </summary>
    /// <param name="authorizationHelper">An instance used to assist with authorization logic for user applications.</param>
    public AllowedApplicationHandler(IAuthorizationHelper authorizationHelper)
        => _authorizationHelper = authorizationHelper;

    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, AllowedApplicationRequirement requirement)
    {
        var allowed = _authorizationHelper.TryGetUmbracoUser(context.User, out IUser? user)
                      && user.AllowedSections.ContainsAny(requirement.Applications);
        return Task.FromResult(allowed);
    }
}
