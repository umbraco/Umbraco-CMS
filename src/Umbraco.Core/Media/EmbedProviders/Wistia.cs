using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Wistia the video hosting and marketing platform provider.
/// </summary>
public class Wistia : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Wistia"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public Wistia(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://fast.wistia.com/oembed";

    /// <inheritdoc/>
    public override string[] UrlSchemeRegex =>
    [
        @"^https?:\/\/([a-z0-9-]+\.)*wistia\.com\/(medias|m|s)\/",
        @"^https?:\/\/([a-z0-9-]+\.)*wistia\.com\/embed\/channel\/",
        @"^https?:\/\/(www\.)?wi\.st\/(medias|m|s)\/",
    ];

    /// <inheritdoc/>
    public override Dictionary<string, string> RequestParams => new()
    {
        // ApiUrl/?format=json
        { "format", "json" },
    };

    /// <inheritdoc/>
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
