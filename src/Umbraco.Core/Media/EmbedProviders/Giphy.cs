using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Giphy.com the popular online GIFs and animated sticker provider.
/// </summary>
public class Giphy : OEmbedProviderBase
{
    public Giphy(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://giphy.com/services/oembed?url=";

    public override string[] UrlSchemeRegex => new[] { @"giphy\.com/*", @"gph\.is/*" };

    public override Dictionary<string, string> RequestParams => new();

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
