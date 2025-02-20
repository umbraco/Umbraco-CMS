using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Issuu the popular platform to create interactive flipbooks, social media posts, GIFs, and more from a single piece of static content.
/// </summary>
public class Issuu : OEmbedProviderBase
{
    public Issuu(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://issuu.com/oembed";

    public override string[] UrlSchemeRegex => new[] { @"issuu.com/.*/docs/.*" };

    public override Dictionary<string, string> RequestParams => new()
    {
        // ApiUrl/?iframe=true
        { "iframe", "true" },

        // ApiUrl/?format=xml
        { "format", "xml" },
    };

    [Obsolete("Use GetMarkupAsync instead. This will be removed in Umbraco 15.")]
    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        => GetMarkupAsync(url, maxWidth, maxHeight, CancellationToken.None).GetAwaiter().GetResult();

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);

    [Obsolete("Use GetMarkupAsync instead. Planned for removal in v16")]
    public override async Task<string?> GeOEmbedDataAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        XmlDocument xmlDocument = await base.GetXmlResponseAsync(requestUrl, cancellationToken);

        return GetXmlProperty(xmlDocument, "/oembed/html");
    }
}
