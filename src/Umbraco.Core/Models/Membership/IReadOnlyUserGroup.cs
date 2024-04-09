using Umbraco.Cms.Core.Models.Membership.Permissions;

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

    ISet<string> Permissions { get; }

    ISet<IGranularPermission> GranularPermissions { get; }

    IEnumerable<string> AllowedSections { get; }

    IEnumerable<int> AllowedLanguages => Enumerable.Empty<int>();

    public bool HasAccessToLanguage( int languageId) => HasAccessToAllLanguages || AllowedLanguages.Contains(languageId);
}
