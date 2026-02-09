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
    ///     Content type changes impact only the Content type being saved
    /// </summary>
    RefreshMain = 2,

    /// <summary>
    ///     Content type changes impacts the content type being saved and others used that are composed of it
    /// </summary>
    RefreshOther = 4, // changed, other change

    /// <summary>
    ///     Content type was removed
    /// </summary>
    Remove = 8,
}
