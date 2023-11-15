namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     A readonly user group providing basic information
/// </summary>
public interface IReadOnlyUserGroup
{
    string? Name { get; }

    string? Icon { get; }

    int Id { get; }

    Guid Key => Guid.Empty;

    int? StartContentId { get; }

    int? StartMediaId { get; }

    /// <summary>
    ///     The alias
    /// </summary>
    string Alias { get; }

    // This is set to return true as default to avoid breaking changes.
    bool HasAccessToAllLanguages => true;

    /// <summary>
    ///     The set of default permissions
    /// </summary>
    /// <remarks>
    ///     By default each permission is simply a single char but we've made this an enumerable{string} to support a more
    ///     flexible permissions structure in the future.
    /// </remarks>
    IEnumerable<string>? Permissions { get; set; } // todo v14 remove when old backoffice is removed, is superseded by ContextualPermissions

    IEnumerable<string> AllowedSections { get; } // todo v14 try to move this into ContextualPermissions

    IEnumerable<int> AllowedLanguages => Enumerable.Empty<int>();

    public bool HasAccessToLanguage(int languageId) => HasAccessToAllLanguages || AllowedLanguages.Contains(languageId);

    ISet<ContextualPermission> ContextualPermissions { get; } // todo v14 rename to permissions when others have been cleaned up
}
