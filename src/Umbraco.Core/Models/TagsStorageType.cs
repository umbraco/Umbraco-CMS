namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines how tags are stored.
/// </summary>
/// <remarks>
///     Tags are always stored as a string, but the string can
///     either be a delimited string, or a serialized Json array.
/// </remarks>
public enum TagsStorageType
{
    /// <summary>
    ///     Store tags as a delimited string.
    /// </summary>
    Csv,

    /// <summary>
    ///     Store tags as serialized Json.
    /// </summary>
    Json,
}
