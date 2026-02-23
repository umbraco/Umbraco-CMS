namespace Umbraco.Cms.Core.Services.Changes;

/// <summary>
///     Represents the types of changes that can occur on a content type.
/// </summary>
[Flags]
public enum ContentTypeChangeTypes : byte
{
    /// <summary>
    ///     No change has occurred.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Item type has been created, no impact
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
    ///     Content type was removed
    /// </summary>
    Remove = 8,
}
