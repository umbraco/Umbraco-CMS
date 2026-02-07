using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for checking dictionary item culture permissions for users.
/// </summary>
public class DictionaryPermissionService : IDictionaryPermissionService
{
    private readonly ILanguageService _languageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryPermissionService"/> class.
    /// </summary>
    /// <param name="languageService">The language service.</param>
    public DictionaryPermissionService(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    /// <inheritdoc/>
    public async Task<DictionaryAuthorizationStatus> AuthorizeCultureAccessAsync(IUser user, ISet<string> culturesToCheck)
    {
        if (user.Groups.Any(group => group.HasAccessToAllLanguages))
        {
            return DictionaryAuthorizationStatus.Success;
        }

        var allowedLanguages = user.Groups.SelectMany(g => g.AllowedLanguages).Distinct().ToArray();
        var allowedLanguageIsoCodes = await _languageService.GetIsoCodesByIdsAsync(allowedLanguages);

        return culturesToCheck.All(culture => allowedLanguageIsoCodes.InvariantContains(culture))
            ? DictionaryAuthorizationStatus.Success
            : DictionaryAuthorizationStatus.UnauthorizedMissingCulture;
    }
}
