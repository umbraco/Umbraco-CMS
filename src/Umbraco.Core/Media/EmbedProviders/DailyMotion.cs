using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Dailymotion the popular online video-sharing platform.
/// </summary>
public class DailyMotion : EmbedProviderBase
{
    public DailyMotion(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://www.dailymotion.com/services/oembed";

    public override string[] UrlSchemeRegex => new[] { @"dailymotion.com/video/.*" };

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
