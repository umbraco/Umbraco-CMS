using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Dailymotion the popular online video-sharing platform.
/// </summary>
public class DailyMotion : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DailyMotion"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public DailyMotion(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://www.dailymotion.com/services/oembed";

    /// <inheritdoc/>
    public override string[] UrlSchemeRegex => [@"^https?:\/\/(www\.)?dailymotion\.com\/video\/"];

    /// <inheritdoc/>
    public override Dictionary<string, string> RequestParams => new()
    {
        // ApiUrl/?format=xml
        { "format", "xml" },
    };

    /// <inheritdoc/>
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetXmlBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
