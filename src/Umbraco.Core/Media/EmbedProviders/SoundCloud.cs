using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for SoundCloud the popular online audio distribution platform and music sharing provider.
/// </summary>
public class Soundcloud : OEmbedProviderBase
{
    public Soundcloud(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://soundcloud.com/oembed";

    public override string[] UrlSchemeRegex => new[] { @"soundcloud.com\/*" };

    public override Dictionary<string, string> RequestParams => new();

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
