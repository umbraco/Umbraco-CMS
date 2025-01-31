using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for YouTube the popular online video sharing and social media platform provider.
/// </summary>
public class YouTube : OEmbedProviderBase
{
    public YouTube(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://www.youtube.com/oembed";

    public override string[] UrlSchemeRegex => new[] { @"youtu.be/.*", @"youtube.com/watch.*", @"youtube.com/shorts/.*", @"youtube.com/live/.*" };

    public override Dictionary<string, string> RequestParams => new()
    {
        // ApiUrl/?format=json
        { "format", "json" },
    };

    [Obsolete("Use GetMarkupAsync instead. This will be removed in Umbraco 15.")]
    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        => GetMarkupAsync(url, maxWidth, maxHeight, CancellationToken.None).GetAwaiter().GetResult();

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);

    [Obsolete("Use GetMarkupAsync instead. Planned for removal in v16")]
    public override async Task<string?> GeOEmbedDataAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse? oembed = await base.GetJsonResponseAsync<OEmbedResponse>(requestUrl, cancellationToken);

        return oembed?.GetHtml();
    }
}
