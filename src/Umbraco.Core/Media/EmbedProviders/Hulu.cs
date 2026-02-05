using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Hulu the the popular online subscription streaming service.
/// </summary>
public class Hulu : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Hulu"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public Hulu(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://www.hulu.com/api/oembed.json";

    /// <inheritdoc/>
    public override string[] UrlSchemeRegex => [@"^https?:\/\/(www\.)?hulu\.com\/watch\/"];

    /// <inheritdoc/>
    public override Dictionary<string, string> RequestParams => [];

    /// <inheritdoc/>
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
