namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     The type of published element.
/// </summary>
/// <remarks>Can be a simple element, or a document, a media, a member.</remarks>
public enum PublishedItemType
{
    /// <summary>
    ///     Unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     An element.
    /// </summary>
    Element,

    /// <summary>
    ///     A document.
    /// </summary>
    Content,

    /// <summary>
    ///     A media.
    /// </summary>
    Media,

    /// <summary>
    ///     A member.
    /// </summary>
    Member,
}
