namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     A readonly user group providing basic information
/// </summary>
public interface IReadOnlyUserGroup
{
    string? Name { get; }

    string? Icon { get; }

    int Id { get; }

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
    IEnumerable<string>? Permissions { get; set; }

    IEnumerable<string> AllowedSections { get; }

    IEnumerable<int> AllowedLanguages => Enumerable.Empty<int>();

    public bool HasAccessToLanguage( int languageId) => HasAccessToAllLanguages || AllowedLanguages.Contains(languageId);
}
