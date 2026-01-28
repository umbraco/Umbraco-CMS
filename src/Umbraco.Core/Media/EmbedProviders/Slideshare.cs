using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for SlideShare for professional online content including presentations, infographics, documents, and videos.
/// </summary>
public class Slideshare : OEmbedProviderBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Slideshare"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public Slideshare(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    public override string ApiEndpoint => "http://www.slideshare.net/api/oembed/2";

    /// <inheritdoc />
    public override string[] UrlSchemeRegex => new[] { @"slideshare\.net/" };

    /// <inheritdoc />
    public override Dictionary<string, string> RequestParams => new();

    /// <inheritdoc />
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
