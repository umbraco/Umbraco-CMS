using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class DictionaryPermissionAuthorizer : IDictionaryPermissionAuthorizer
{
    private readonly IDictionaryPermissionService _dictionaryPermissionService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryPermissionAuthorizer" /> class.
    /// </summary>
    /// <param name="dictionaryPermissionService">The dictionary permission service.</param>
    public DictionaryPermissionAuthorizer(IDictionaryPermissionService dictionaryPermissionService) =>
        _dictionaryPermissionService = dictionaryPermissionService;

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedForCultures(IUser currentUser, ISet<string> culturesToCheck)
    {
        DictionaryAuthorizationStatus result =
            await _dictionaryPermissionService.AuthorizeCultureAccessAsync(currentUser, culturesToCheck);
        return result is DictionaryAuthorizationStatus.Success;
    }
}
