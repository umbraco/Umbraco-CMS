using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for X the popular online service for microblogging and social networking.
/// </summary>
public class X : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="X"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public X(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://publish.x.com/oembed";

    /// <inheritdoc/>
    public override string[] UrlSchemeRegex => [@"^https?:\/\/(www\.)?(twitter|x)\.com\/.+\/status\/"];

    /// <inheritdoc/>
    public override Dictionary<string, string> RequestParams => [];

    /// <inheritdoc/>
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
