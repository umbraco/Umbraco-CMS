using System.Net;
using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Flickr the popular online image hosting and video hosting service.
/// </summary>
public class Flickr : EmbedProviderBase
{
    public Flickr(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://www.flickr.com/services/oembed/";

    public override string[] UrlSchemeRegex => new[] { @"flickr.com\/photos\/*", @"flic.kr\/p\/*" };

    public override Dictionary<string, string> RequestParams => new();

    public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = base.GetXmlResponse(requestUrl);

        var imageUrl = GetXmlProperty(xmlDocument, "/oembed/url");
        var imageWidth = GetXmlProperty(xmlDocument, "/oembed/width");
        var imageHeight = GetXmlProperty(xmlDocument, "/oembed/height");
        var imageTitle = GetXmlProperty(xmlDocument, "/oembed/title");

        return string.Format("<img src=\"{0}\" width=\"{1}\" height=\"{2}\" alt=\"{3}\" />", imageUrl, imageWidth, imageHeight, WebUtility.HtmlEncode(imageTitle));
    }
}
