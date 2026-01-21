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
    ///     Initializes a new instance of the <see cref="Flickr"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public Flickr(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    public override string ApiEndpoint => "http://www.flickr.com/services/oembed/";

    /// <inheritdoc />
    public override string[] UrlSchemeRegex => new[] { @"flickr.com\/photos\/*", @"flic.kr\/p\/*" };

    /// <inheritdoc />
    public override Dictionary<string, string> RequestParams => new();

    /// <inheritdoc />
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = await base.GetXmlResponseAsync(requestUrl, cancellationToken);

        var imageUrl = GetXmlProperty(xmlDocument, "/oembed/url");
        var imageWidth = GetXmlProperty(xmlDocument, "/oembed/width");
        var imageHeight = GetXmlProperty(xmlDocument, "/oembed/height");
        var imageTitle = GetXmlProperty(xmlDocument, "/oembed/title");

        return string.Format("<img src=\"{0}\" width=\"{1}\" height=\"{2}\" alt=\"{3}\" />", imageUrl, imageWidth, imageHeight, WebUtility.HtmlEncode(imageTitle));
    }
}
