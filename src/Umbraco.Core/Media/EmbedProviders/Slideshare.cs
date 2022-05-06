using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

// TODO(V10) : change base class to OEmbedProviderBase
public class Slideshare : EmbedProviderBase
{
    public Slideshare(IJsonSerializer jsonSerializer) : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://www.slideshare.net/api/oembed/2";

    public override string[] UrlSchemeRegex => new[] {@"slideshare\.net/"};

    public override Dictionary<string, string> RequestParams => new();

    public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = base.GetXmlResponse(requestUrl);

        return GetXmlProperty(xmlDocument, "/oembed/html");
    }
}
