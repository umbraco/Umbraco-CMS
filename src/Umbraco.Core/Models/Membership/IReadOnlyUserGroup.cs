using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     A readonly user group providing basic information
/// </summary>
public interface IReadOnlyUserGroup
{
    /// <summary>
    ///     Gets the name of the user group.
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     Gets the alias of the user group.
    /// </summary>
    string Alias { get; }

    /// <summary>
    ///     Gets the description of the user group.
    /// </summary>
    /// <remarks>
    ///     TODO (V18): Remove the default implementations.
    /// </remarks>
    string? Description { get { return null; } }

    /// <summary>
    ///     Gets the icon for the user group.
    /// </summary>
    string? Icon { get; }

    /// <summary>
    ///     Gets the unique identifier for the user group.
    /// </summary>
    int Id { get; }

    /// <summary>
    ///     Gets the unique key for the user group.
    /// </summary>
    Guid Key => Guid.Empty;

    /// <summary>
    ///     Gets the starting content node identifier for this user group.
    /// </summary>
    int? StartContentId { get; }

    /// <summary>
    ///     Gets the starting media node identifier for this user group.
    /// </summary>
    int? StartMediaId { get; }

    /// <summary>
    ///     Gets a value indicating whether this user group has access to all languages.
    /// </summary>
    /// <remarks>
    ///     This is set to return true as default to avoid breaking changes.
    /// </remarks>
    bool HasAccessToAllLanguages => true;

    /// <summary>
    ///     Gets the set of permissions assigned to this user group.
    /// </summary>
    ISet<string> Permissions { get; }

    /// <summary>
    ///     Gets the set of granular permissions assigned to this user group.
    /// </summary>
    ISet<IGranularPermission> GranularPermissions { get; }

    /// <summary>
    ///     Gets the collection of section aliases that this user group has access to.
    /// </summary>
    IEnumerable<string> AllowedSections { get; }

    /// <summary>
    ///     Gets the collection of language identifiers that this user group has access to.
    /// </summary>
    IEnumerable<int> AllowedLanguages => Enumerable.Empty<int>();

    /// <summary>
    ///     Determines whether this user group has access to the specified language.
    /// </summary>
    /// <param name="languageId">The language identifier to check.</param>
    /// <returns><c>true</c> if the user group has access to the language; otherwise, <c>false</c>.</returns>
    public bool HasAccessToLanguage( int languageId) => HasAccessToAllLanguages || AllowedLanguages.Contains(languageId);
}
