using System.Xml;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Dailymotion the popular online video-sharing platform.
/// </summary>
public class DailyMotion : OEmbedProviderBase
{
    public DailyMotion(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://www.dailymotion.com/services/oembed";

    public override string[] UrlSchemeRegex => new[] { @"dailymotion.com/video/.*" };

    public override Dictionary<string, string> RequestParams => new()
    {
        // ApiUrl/?format=xml
        { "format", "xml" },
    };

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
