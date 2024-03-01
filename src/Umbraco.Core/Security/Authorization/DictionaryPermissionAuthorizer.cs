using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

internal class DictionaryPermissionAuthorizer : IDictionaryPermissionAuthorizer
{
    private readonly IDictionaryPermissionService _dictionaryPermissionService;

    public DictionaryPermissionAuthorizer(IDictionaryPermissionService dictionaryPermissionService) =>
        _dictionaryPermissionService = dictionaryPermissionService;

    public async Task<bool> IsAuthorizedForCultures(IUser currentUser, ISet<string> culturesToCheck)
    {
        DictionaryAuthorizationStatus result =
            await _dictionaryPermissionService.AuthorizeCultureAccessAsync(currentUser, culturesToCheck);
        return result is DictionaryAuthorizationStatus.Success;
    }
}
