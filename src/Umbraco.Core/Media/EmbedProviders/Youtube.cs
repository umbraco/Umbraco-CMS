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

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
