using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Giphy.com the popular online GIFs and animated sticker provider.
/// </summary>
public class Giphy : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Giphy"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public Giphy(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://giphy.com/services/oembed?url=";

    /// <inheritdoc/>
    public override string[] UrlSchemeRegex =>
    [
        @"^https?:\/\/(www\.)?giphy\.com\/",
        @"^https?:\/\/(www\.)?gph\.is\/",
    ];

    /// <inheritdoc/>
    public override Dictionary<string, string> RequestParams => [];

    /// <inheritdoc/>
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
