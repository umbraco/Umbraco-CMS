using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a link display model for content editing.
/// </summary>
[DataContract(Name = "link", Namespace = "")]
public class LinkDisplay
{
    /// <summary>
    ///     Gets or sets the icon for the link.
    /// </summary>
    [DataMember(Name = "icon")]
    public string? Icon { get; set; }

    /// <summary>
    ///     Gets or sets the display name of the link.
    /// </summary>
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the linked content is published.
    /// </summary>
    [DataMember(Name = "published")]
    public bool Published { get; set; }

    /// <summary>
    ///     Gets or sets the query string portion of the link URL.
    /// </summary>
    [DataMember(Name = "queryString")]
    public string? QueryString { get; set; }

    /// <summary>
    ///     Gets or sets the target attribute for the link (e.g., "_blank").
    /// </summary>
    [DataMember(Name = "target")]
    public string? Target { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the linked content is in the recycle bin.
    /// </summary>
    [DataMember(Name = "trashed")]
    public bool Trashed { get; set; }

    /// <summary>
    ///     Gets or sets the type of link (document, media, or external).
    /// </summary>
    [DataMember(Name = "type")]
    public string? Type { get; set; }

    /// <summary>
    ///     Gets or sets the unique identifier of the linked content.
    /// </summary>
    [DataMember(Name = "unique")]
    public Guid? Unique { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the link.
    /// </summary>
    [DataMember(Name = "url")]
    public string? Url { get; set; }

    /// <summary>
    ///     Contains constants for link types.
    /// </summary>
    public static class Types
    {
        /// <summary>
        ///     Represents a link to a document.
        /// </summary>
        public const string Document = "document";

        /// <summary>
        ///     Represents a link to a media item.
        /// </summary>
        public const string Media = "media";

        /// <summary>
        ///     Represents an external link.
        /// </summary>
        public const string External = "external";
    }
}
