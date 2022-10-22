using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for SoundCloud the popular online audio distribution platform and music sharing provider.
/// </summary>
public class Soundcloud : EmbedProviderBase
{
    public Soundcloud(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://soundcloud.com/oembed";

    public override string[] UrlSchemeRegex => new[] { @"soundcloud.com\/*" };

    public override Dictionary<string, string> RequestParams => new();

    public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = base.GetXmlResponse(requestUrl);

        return GetXmlProperty(xmlDocument, "/oembed/html");
    }
}
