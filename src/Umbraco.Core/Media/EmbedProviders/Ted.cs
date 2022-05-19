using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

// TODO(V10) : change base class to OEmbedProviderBase
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
        var requestUrl = this.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = this.GetXmlResponse(requestUrl);

        return GetXmlProperty(xmlDocument, "/oembed/html");
    }
}
