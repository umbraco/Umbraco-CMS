using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Dictionary;

/// <summary>
///     Authorizes that the current user has the correct permission access to the dictionary item(s) specified in the request.
/// </summary>
public class DictionaryPermissionHandler : MustSatisfyRequirementAuthorizationHandler<DictionaryPermissionRequirement, DictionaryPermissionResource>
{
    private readonly IDictionaryPermissionAuthorizer _dictionaryPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryPermissionHandler" /> class.
    /// </summary>
    /// <param name="dictionaryPermissionAuthorizer">Authorizer for content access.</param>
    public DictionaryPermissionHandler(IDictionaryPermissionAuthorizer dictionaryPermissionAuthorizer)
        => _dictionaryPermissionAuthorizer = dictionaryPermissionAuthorizer;

    protected override async Task<bool> IsAuthorized(AuthorizationHandlerContext context, DictionaryPermissionRequirement requirement,
        DictionaryPermissionResource resource)
    {
        if (resource.CulturesToCheck.Any()
            && await _dictionaryPermissionAuthorizer.IsAuthorizedForCultures(context.User, resource.CulturesToCheck) is false)
        {
            return false;
        }

        return true;
    }
}
