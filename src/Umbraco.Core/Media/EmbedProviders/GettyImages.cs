using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Getty Images supplier of stock images, editorial photography, video and music for business and consumers.
/// </summary>
public class GettyImages : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GettyImages"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public GettyImages(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://embed.gettyimages.com/oembed";

    /// <inheritdoc/>
    /// <example>
    /// https://gty.im/74917285
    /// https://www.gettyimages.com/detail/74917285
    /// </example>
    public override string[] UrlSchemeRegex =>
    [
        @"^https?:\/\/(www\.)?gty\.im\/",
        @"^https?:\/\/(www\.)?gettyimages\.com\/detail\/",
    ];

    /// <inheritdoc/>
    public override Dictionary<string, string> RequestParams => [];

    /// <inheritdoc/>
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
