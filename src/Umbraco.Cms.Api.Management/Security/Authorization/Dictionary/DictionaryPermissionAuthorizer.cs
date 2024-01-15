using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Dictionary;

public class DictionaryPermissionAuthorizer : IDictionaryPermissionAuthorizer
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IDictionaryPermissionService _dictionaryPermissionService;

    public DictionaryPermissionAuthorizer(IAuthorizationHelper authorizationHelper, IDictionaryPermissionService dictionaryPermissionService)
    {
        _authorizationHelper = authorizationHelper;
        _dictionaryPermissionService = dictionaryPermissionService;
    }

    public async Task<bool> IsAuthorizedForCultures(IPrincipal currentUser, ISet<string> culturesToCheck)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(currentUser);
        DictionaryAuthorizationStatus result = await _dictionaryPermissionService.AuthorizeCultureAccessAsync(user, culturesToCheck);
        return result == DictionaryAuthorizationStatus.Success;
    }
}
