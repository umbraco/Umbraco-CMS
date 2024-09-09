using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for SlideShare for professional online content including presentations, infographics, documents, and videos.
/// </summary>
public class Slideshare : OEmbedProviderBase
{
    public Slideshare(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://www.slideshare.net/api/oembed/2";

    public override string[] UrlSchemeRegex => new[] { @"slideshare\.net/" };

    public override Dictionary<string, string> RequestParams => new();

    [Obsolete("Use GetOEmbedDataAsync instead. This will be removed in Umbraco 15.")]
    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        return GetOEmbedDataAsync(url, maxWidth, maxHeight, CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<string?> GetOEmbedDataAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = await base.GetXmlResponseAsync(requestUrl, cancellationToken);

        return GetXmlProperty(xmlDocument, "/oembed/html");
    }
}
