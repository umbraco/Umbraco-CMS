namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Enum representing the taggable object types
/// </summary>
public enum TaggableObjectTypes
{
    /// <summary>
    /// Includes all taggable object types.
    /// </summary>
    All,

    /// <summary>
    /// Represents content entities (documents/pages).
    /// </summary>
    Content,

    /// <summary>
    /// Represents media entities (images, files, etc.).
    /// </summary>
    Media,

    /// <summary>
    /// Represents member entities (user accounts).
    /// </summary>
    Member,
}
