namespace Umbraco.Cms.Core.Services.Changes;

/// <summary>
///     Represents the types of changes that can occur on a content type.
/// </summary>
/// <remarks>
///     <para>
///         The base flags (<see cref="RefreshMain"/> and <see cref="RefreshOther"/>) represent coarse-grained
///         structural vs non-structural change categories. The granular flags (e.g. <see cref="AliasChanged"/>,
///         <see cref="PropertyRemoved"/>) include their parent flag via bitwise OR, so existing consumers that
///         only check for <see cref="RefreshMain"/> or <see cref="RefreshOther"/> continue to work unchanged.
///     </para>
/// </remarks>
[Flags]
public enum ContentTypeChangeTypes : ushort
{
    /// <summary>
    ///     No change has occurred.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Item type has been created, no impact.
    /// </summary>
    Create = 1,

    /// <summary>
    ///     Content type changes directly impact existing content of this content type.
    /// </summary>
    /// <remarks>
    ///     These changes are "destructive" of nature. They include:
    ///     - Changing the content type alias.
    ///     - Removing a property type or a composition.
    ///     - Changing the alias of a property type (this effectively corresponds to removing a property type).
    ///     - Changing variance, either at property or content type level.
    /// </remarks>
    RefreshMain = 2,

    /// <summary>
    ///     Content type changes that do not directly impact existing content of this content type.
    /// </summary>
    /// <remarks>
    ///     These changes are "constructive" of nature, and include all changes not included in
    ///     <see cref="RefreshMain"/> - for example:
    ///     - Adding a property type or a composition.
    ///     - Rearranging property types or groups.
    ///     - Changes to name, description, icon etc.
    ///     - Changes to other content type settings, i.e. allowed child types and version cleanup.
    /// </remarks>
    RefreshOther = 4,

    /// <summary>
    ///     Content type was removed.
    /// </summary>
    Remove = 8,

    /// <summary>
    ///     Content type variation setting has changed (e.g., from invariant to variant or vice versa).
    ///     This impacts how URL segments and aliases are stored (NULL languageId for invariant, specific ID for variant).
    /// </summary>
    VariationChanged = 16 | RefreshMain,

    // Granular structural flags (always include RefreshMain)

    /// <summary>
    ///     The content type alias was changed.
    /// </summary>
    AliasChanged = 32 | RefreshMain,

    /// <summary>
    ///     One or more property type aliases were changed.
    ///     This effectively corresponds to removing and re-adding a property type.
    /// </summary>
    PropertyAliasChanged = 64 | RefreshMain,

    /// <summary>
    ///     One or more property types were removed from the content type.
    /// </summary>
    PropertyRemoved = 128 | RefreshMain,

    /// <summary>
    ///     One or more compositions were removed from the content type.
    /// </summary>
    CompositionRemoved = 256 | RefreshMain,

    /// <summary>
    ///     The variation setting of one or more property types changed
    ///     (e.g., from invariant to variant or vice versa).
    /// </summary>
    PropertyVariationChanged = 512 | RefreshMain,

    // Granular non-structural flags (always include RefreshOther)

    /// <summary>
    ///     One or more property types were added to the content type.
    /// </summary>
    PropertyAdded = 1024 | RefreshOther,

    /// <summary>
    ///     One or more compositions were added to the content type.
    /// </summary>
    CompositionAdded = 2048 | RefreshOther,

    /// <summary>
    ///     Non-property metadata changed (name, description, icon, thumbnail, allowed templates, etc.).
    /// </summary>
    MetadataChanged = 4096 | RefreshOther,

    /// <summary>
    ///     Structure settings changed (allowed child content types, sort order, list view configuration, etc.).
    /// </summary>
    StructureSettingsChanged = 8192 | RefreshOther,
}
