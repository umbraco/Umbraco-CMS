using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Ted that posts talks online for free distribution.
/// </summary>
public class Ted : EmbedProviderBase
{
    public Ted(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://www.ted.com/talks/oembed.xml";

    public override string[] UrlSchemeRegex => new[] { @"ted.com\/talks\/*" };

    public override Dictionary<string, string> RequestParams => new();

    public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = base.GetXmlResponse(requestUrl);

        return GetXmlProperty(xmlDocument, "/oembed/html");
    }
}
