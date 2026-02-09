using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for YouTube the popular online video sharing and social media platform provider.
/// </summary>
public class YouTube : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YouTube"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public YouTube(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://www.youtube.com/oembed";

    /// <inheritdoc/>
    public override string[] UrlSchemeRegex =>
    [
        @"^https?:\/\/(www\.)?youtu\.be\/",
        @"^https?:\/\/(www\.)?youtube\.com\/watch",
        @"^https?:\/\/(www\.)?youtube\.com\/shorts\/",
        @"^https?:\/\/(www\.)?youtube\.com\/live\/",
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
