using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Vimeo the popular online video hosting, sharing, and services platform provider.
/// </summary>
public class Vimeo : OEmbedProviderBase
{
    public Vimeo(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://vimeo.com/api/oembed.xml";

    public override string[] UrlSchemeRegex => new[] { @"vimeo\.com/" };

    public override Dictionary<string, string> RequestParams => new();

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
