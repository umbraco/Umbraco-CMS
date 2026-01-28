using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Vimeo the popular online video hosting, sharing, and services platform provider.
/// </summary>
public class Vimeo : OEmbedProviderBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Vimeo"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public Vimeo(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    public override string ApiEndpoint => "https://vimeo.com/api/oembed.xml";

    /// <inheritdoc />
    public override string[] UrlSchemeRegex => new[] { @"vimeo\.com/" };

    /// <inheritdoc />
    public override Dictionary<string, string> RequestParams => new();

    /// <inheritdoc />
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
