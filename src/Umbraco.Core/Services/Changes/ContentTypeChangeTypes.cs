namespace Umbraco.Cms.Core.Services.Changes;

[Flags]
public enum ContentTypeChangeTypes : byte
{
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
