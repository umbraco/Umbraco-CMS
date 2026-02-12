using System.Collections;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
/// Represents a user group in Umbraco.
/// </summary>
public interface IUserGroup : IEntity, IRememberBeingDirty
{
    /// <summary>
    /// Gets or sets the alias of the user group.
    /// </summary>
    string Alias { get; set; }

    /// <summary>
    /// Gets or sets the starting content node ID for this user group.
    /// </summary>
    int? StartContentId { get; set; }

    /// <summary>
    /// Gets or sets the starting media node ID for this user group.
    /// </summary>
    int? StartMediaId { get; set; }

    /// <summary>
    ///     The icon
    /// </summary>
    string? Icon { get; set; }

    /// <summary>
    ///     The name
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the user group.
    /// </summary>
    /// <remarks>
    /// TODO (V18): Remove the default implementations.
    /// </remarks>
    string? Description
    {
        get => null;
        set { }
    }

    /// <summary>
    ///     If this property is true it will give the group access to all languages
    /// </summary>
    /// This is set to return true as default to avoid breaking changes
    public bool HasAccessToAllLanguages
    {
        get => true;
        set { /* This is NoOp to avoid breaking changes */ }
    }

    /// <summary>
    /// The set of permissions provided by the frontend.
    /// </summary>
    /// <remarks>
    /// By default the server has no concept of what all of these strings mean, we simple store them and return them to the UI.
    /// </remarks>
    ISet<string> Permissions { get; set; }

    /// <summary>
    /// Gets or sets the granular permissions for this user group.
    /// </summary>
    ISet<IGranularPermission> GranularPermissions { get; set; }

    /// <summary>
    /// Gets the collection of section aliases that this user group has access to.
    /// </summary>
    IEnumerable<string> AllowedSections { get; }

    /// <summary>
    /// Removes access to a section for this user group.
    /// </summary>
    /// <param name="sectionAlias">The alias of the section to remove.</param>
    void RemoveAllowedSection(string sectionAlias);

    /// <summary>
    /// Adds access to a section for this user group.
    /// </summary>
    /// <param name="sectionAlias">The alias of the section to add.</param>
    void AddAllowedSection(string sectionAlias);

    /// <summary>
    /// Removes access to all sections for this user group.
    /// </summary>
    void ClearAllowedSections();

    /// <summary>
    /// Gets the collection of language IDs that this user group has access to.
    /// </summary>
    IEnumerable<int> AllowedLanguages => Enumerable.Empty<int>();

    /// <summary>
    /// Removes access to a language for this user group.
    /// </summary>
    /// <param name="languageId">The ID of the language to remove.</param>
    void RemoveAllowedLanguage(int languageId)
    {
    }

    /// <summary>
    /// Adds access to a language for this user group.
    /// </summary>
    /// <param name="languageId">The ID of the language to add.</param>
    void AddAllowedLanguage(int languageId)
    {
    }

    /// <summary>
    /// Removes access to all languages for this user group.
    /// </summary>
    void ClearAllowedLanguages()
    {
    }

    /// <summary>
    ///     Specifies the number of users assigned to this group
    /// </summary>
    int UserCount { get; }
}
