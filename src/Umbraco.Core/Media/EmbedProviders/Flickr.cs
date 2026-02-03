using System.Net;
using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Flickr the popular online image hosting and video hosting service.
/// </summary>
public class Flickr : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Flickr"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public Flickr(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://www.flickr.com/services/oembed/";

    /// <inheritdoc/>
    public override string[] UrlSchemeRegex => [@"^https?:\/\/(www\.)?flickr\.com\/photos\/"];

    /// <inheritdoc/>
    public override Dictionary<string, string> RequestParams => [];

    /// <inheritdoc/>
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = await GetXmlResponseAsync(requestUrl, cancellationToken);

        return BuildMarkup(xmlDocument);
    }

    /// <summary>
    /// Builds an img tag from the oEmbed XML response.
    /// </summary>
    /// <param name="xmlDocument">The XML document from the oEmbed API.</param>
    /// <returns>An HTML img tag string.</returns>
    /// <remarks>Internal to support unit testing.</remarks>
    internal string BuildMarkup(XmlDocument xmlDocument)
    {
        var imageUrl = GetXmlProperty(xmlDocument, "/oembed/url");
        var imageWidth = GetXmlProperty(xmlDocument, "/oembed/width");
        var imageHeight = GetXmlProperty(xmlDocument, "/oembed/height");
        var imageTitle = GetXmlProperty(xmlDocument, "/oembed/title");

        return string.Format(
            "<img src=\"{0}\" width=\"{1}\" height=\"{2}\" alt=\"{3}\" />",
            imageUrl,
            imageWidth,
            imageHeight,
            WebUtility.HtmlEncode(imageTitle));
    }
}
