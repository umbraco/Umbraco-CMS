using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a link that can point to content, media, or external URLs.
/// </summary>
public class Link
{
    /// <summary>
    ///     Gets or sets the display name of the link.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the target attribute for the link (e.g., "_blank" for new window).
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    ///     Gets or sets the type of link (Content, Media, or External).
    /// </summary>
    public LinkType Type { get; set; }

    /// <summary>
    ///     Gets or sets the Umbraco Data Identifier for content or media links.
    /// </summary>
    public Udi? Udi { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the link.
    /// </summary>
    public string? Url { get; set; }
}
