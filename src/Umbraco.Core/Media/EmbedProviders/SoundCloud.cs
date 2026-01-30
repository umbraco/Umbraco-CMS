using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for SoundCloud the popular online audio distribution platform and music sharing provider.
/// </summary>
public class Soundcloud : OEmbedProviderBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Soundcloud"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public Soundcloud(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc />
    public override string ApiEndpoint => "https://soundcloud.com/oembed";

    /// <inheritdoc />
    public override string[] UrlSchemeRegex => new[] { @"soundcloud.com\/*" };

    /// <inheritdoc />
    public override Dictionary<string, string> RequestParams => new();

    /// <inheritdoc />
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
