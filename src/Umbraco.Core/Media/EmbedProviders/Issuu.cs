using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Issuu the popular platform to create interactive flipbooks, social media posts, GIFs, and more from a single piece of static content.
/// </summary>
public class Issuu : EmbedProviderBase
{
    public Issuu(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://issuu.com/oembed";

    public override string[] UrlSchemeRegex => new[] { @"issuu.com/.*/docs/.*" };

    public override Dictionary<string, string> RequestParams => new()
    {
        // ApiUrl/?format=xml
        { "format", "xml" },
    };

    public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = base.GetXmlResponse(requestUrl);

        return GetXmlProperty(xmlDocument, "/oembed/html");
    }
}
