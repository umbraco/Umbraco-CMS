namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Specifies the level of cache for a property value.
/// </summary>
public enum PropertyCacheLevel
{
    /// <summary>
    ///     Default value.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     Indicates that the property value can be cached at the element level, i.e. it can be
    ///     cached until the element itself is modified.
    /// </summary>
    Element = 1,

    /// <summary>
    ///     Indicates that the property value can be cached at the elements level, i.e. it can
    ///     be cached until any element is modified.
    /// </summary>
    Elements = 2,

    /// <summary>
    ///     Indicates that the property value can be cached at the snapshot level, i.e. it can be
    ///     cached for the duration of the current snapshot.
    /// </summary>
    /// <remarks>
    ///     In most cases, a snapshot is created per request, and therefore this is
    ///     equivalent to cache the value for the duration of the request.
    /// </remarks>
    Snapshot = 3,

    /// <summary>
    ///     Indicates that the property value cannot be cached and has to be converted each time
    ///     it is requested.
    /// </summary>
    None = 4,
}
