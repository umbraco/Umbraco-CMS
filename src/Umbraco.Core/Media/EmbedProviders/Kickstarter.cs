using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Kickstarter the popular online crowdfunding platform focused on creativity.
/// </summary>
public class Kickstarter : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Kickstarter"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public Kickstarter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://www.kickstarter.com/services/oembed";

    /// <inheritdoc/>
    public override string[] UrlSchemeRegex => [@"^https?:\/\/(www\.)?kickstarter\.com\/projects\/"];

    /// <inheritdoc/>
    public override Dictionary<string, string> RequestParams => [];

    /// <inheritdoc/>
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
