using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Ted that posts talks online for free distribution.
/// </summary>
public class Ted : OEmbedProviderBase
{
    public Ted(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://www.ted.com/talks/oembed.xml";

    public override string[] UrlSchemeRegex => new[] { @"ted.com\/talks\/*" };

    public override Dictionary<string, string> RequestParams => new();

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
