using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Getty Images supplier of stock images, editorial photography, video and music for business and consumers.
/// </summary>
public class GettyImages : OEmbedProviderBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GettyImages"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public GettyImages(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    public override string ApiEndpoint => "http://embed.gettyimages.com/oembed";

    // http://gty.im/74917285
    // http://www.gettyimages.com/detail/74917285

    /// <inheritdoc />
    public override string[] UrlSchemeRegex => new[] { @"gty\.im/*", @"gettyimages.com\/detail\/*" };

    /// <inheritdoc />
    public override Dictionary<string, string> RequestParams => new();

    /// <inheritdoc />
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
