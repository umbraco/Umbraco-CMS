using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for YouTube the popular online video sharing and social media platform provider.
/// </summary>
public class YouTube : EmbedProviderBase
{
    public YouTube(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://www.youtube.com/oembed";

    public override string[] UrlSchemeRegex => new[] { @"youtu.be/.*", @"youtube.com/watch.*" };

    public override Dictionary<string, string> RequestParams => new()
    {
        // ApiUrl/?format=json
        { "format", "json" },
    };

    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse? oembed = base.GetJsonResponse<OEmbedResponse>(requestUrl);

        return oembed?.GetHtml();
    }
}
