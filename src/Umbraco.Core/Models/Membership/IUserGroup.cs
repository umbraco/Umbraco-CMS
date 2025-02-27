using System.Collections;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Core.Models.Membership;

public interface IUserGroup : IEntity, IRememberBeingDirty
{
    string Alias { get; set; }

    int? StartContentId { get; set; }

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

    ISet<IGranularPermission> GranularPermissions { get; set; }

    IEnumerable<string> AllowedSections { get; }

    void RemoveAllowedSection(string sectionAlias);

    void AddAllowedSection(string sectionAlias);

    void ClearAllowedSections();

    IEnumerable<int> AllowedLanguages => Enumerable.Empty<int>();

    void RemoveAllowedLanguage(int languageId)
    {
    }

    void AddAllowedLanguage(int languageId)
    {
    }

    void ClearAllowedLanguages()
    {
    }

    /// <summary>
    ///     Specifies the number of users assigned to this group
    /// </summary>
    int UserCount { get; }
}
