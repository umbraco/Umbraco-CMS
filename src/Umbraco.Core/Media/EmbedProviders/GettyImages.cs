using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Getty Images supplier of stock images, editorial photography, video and music for business and consumers.
/// </summary>
public class GettyImages : OEmbedProviderBase
{
    public GettyImages(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://embed.gettyimages.com/oembed";

    // http://gty.im/74917285
    // http://www.gettyimages.com/detail/74917285
    public override string[] UrlSchemeRegex => new[] { @"gty\.im/*", @"gettyimages.com\/detail\/*" };

    public override Dictionary<string, string> RequestParams => new();

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
