using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Dictionary;

/// <summary>
///     Authorizes that the current user has the correct permission access to the dictionary item(s) specified in the request.
/// </summary>
public class DictionaryPermissionHandler : MustSatisfyRequirementAuthorizationHandler<DictionaryPermissionRequirement, DictionaryPermissionResource>
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IDictionaryPermissionAuthorizer _dictionaryPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryPermissionHandler" /> class.
    /// </summary>
    /// <param name="dictionaryPermissionAuthorizer">Authorizer for content access.</param>
    /// <param name="authorizationHelper">The authorization helper.</param>
    public DictionaryPermissionHandler(IDictionaryPermissionAuthorizer dictionaryPermissionAuthorizer, IAuthorizationHelper authorizationHelper)
    {
        _dictionaryPermissionAuthorizer = dictionaryPermissionAuthorizer;
        _authorizationHelper = authorizationHelper;
    }

    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        DictionaryPermissionRequirement requirement,
        DictionaryPermissionResource resource)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(context.User);

        if (resource.CulturesToCheck.Any()
            && await _dictionaryPermissionAuthorizer.IsAuthorizedForCultures(user, resource.CulturesToCheck) is false)
        {
            return false;
        }

        return true;
    }
}
