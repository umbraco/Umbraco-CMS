using System.Net;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Base class for OEmbed response.
/// </summary>
[DataContract]
public abstract class OEmbedResponseBase<T>
{
    /// <summary>
    ///     Gets or sets the resource type (e.g., "photo", "video", "link", "rich").
    /// </summary>
    [DataMember(Name = "type")]
    public string? Type { get; set; }

    /// <summary>
    ///     Gets or sets the OEmbed version number.
    /// </summary>
    [DataMember(Name = "version")]
    public string? Version { get; set; }

    /// <summary>
    ///     Gets or sets the title of the resource.
    /// </summary>
    [DataMember(Name = "title")]
    public string? Title { get; set; }

    /// <summary>
    ///     Gets or sets the name of the author/owner of the resource.
    /// </summary>
    [DataMember(Name = "author_name")]
    public string? AuthorName { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the author/owner of the resource.
    /// </summary>
    [DataMember(Name = "author_url")]
    public string? AuthorUrl { get; set; }

    /// <summary>
    ///     Gets or sets the name of the resource provider.
    /// </summary>
    [DataMember(Name = "provider_name")]
    public string? ProviderName { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the resource provider.
    /// </summary>
    [DataMember(Name = "provider_url")]
    public string? ProviderUrl { get; set; }

    /// <summary>
    ///     Gets or sets the URL to a thumbnail image representing the resource.
    /// </summary>
    [DataMember(Name = "thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    ///     Gets or sets the height of the thumbnail image.
    /// </summary>
    [DataMember(Name = "thumbnail_height")]
    public virtual T? ThumbnailHeight { get; set; }

    /// <summary>
    ///     Gets or sets the width of the thumbnail image.
    /// </summary>
    [DataMember(Name = "thumbnail_width")]
    public T? ThumbnailWidth { get; set; }

    /// <summary>
    ///     Gets or sets the HTML snippet for embedding the resource.
    /// </summary>
    [DataMember(Name = "html")]
    public string? Html { get; set; }

    /// <summary>
    ///     Gets or sets the source URL of the resource (used for photo type).
    /// </summary>
    [DataMember(Name = "url")]
    public string? Url { get; set; }

    /// <summary>
    ///     Gets or sets the height of the embedded resource.
    /// </summary>
    [DataMember(Name = "height")]
    public T? Height { get; set; }

    /// <summary>
    ///     Gets or sets the width of the embedded resource.
    /// </summary>
    [DataMember(Name = "width")]
    public T? Width { get; set; }

    /// <summary>
    ///     Gets the HTML.
    /// </summary>
    /// <returns>The response HTML</returns>
    public string GetHtml()
    {
        if (Type == "photo")
        {
            return "<img src=\"" + Url + "\" width=\"" + Width + "\" height=\"" + Height + "\" alt=\"" +
                   WebUtility.HtmlEncode(Title) + "\" />";
        }

        return string.IsNullOrEmpty(Html) == false ? Html : string.Empty;
    }
}
