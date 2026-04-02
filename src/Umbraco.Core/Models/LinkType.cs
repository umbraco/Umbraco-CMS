namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines the type of a link.
/// </summary>
public enum LinkType
{
    /// <summary>
    ///     A link to internal content.
    /// </summary>
    Content,

    /// <summary>
    ///     A link to media.
    /// </summary>
    Media,

    /// <summary>
    ///     A link to an external URL.
    /// </summary>
    External,
}
