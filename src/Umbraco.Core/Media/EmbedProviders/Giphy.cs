using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Giphy.com the popular online GIFs and animated sticker provider.
/// </summary>
public class Giphy : EmbedProviderBase
{
    public Giphy(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://giphy.com/services/oembed?url=";

    public override string[] UrlSchemeRegex => new[] { @"giphy\.com/*", @"gph\.is/*" };

    public override Dictionary<string, string> RequestParams => new();

    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse? oembed = base.GetJsonResponse<OEmbedResponse>(requestUrl);

        return oembed?.GetHtml();
    }
}
