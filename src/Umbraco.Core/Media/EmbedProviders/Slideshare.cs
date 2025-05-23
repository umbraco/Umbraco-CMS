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

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
