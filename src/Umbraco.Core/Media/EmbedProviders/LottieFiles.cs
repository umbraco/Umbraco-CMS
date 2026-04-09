using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for lottiefiles.com the popular opensource JSON-based animation format platform.
/// </summary>
public partial class LottieFiles : OEmbedProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LottieFiles"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public LottieFiles(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    /// <inheritdoc/>
    public override string ApiEndpoint => "https://embed.lottiefiles.com/oembed";

    /// <inheritdoc/>
    public override string[] UrlSchemeRegex => [@"^https?:\/\/(www\.)?lottiefiles\.com\/"];

    /// <inheritdoc/>
    public override Dictionary<string, string> RequestParams => [];

    /// <inheritdoc/>
    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse? oembed = await GetJsonResponseAsync<OEmbedResponse>(requestUrl, cancellationToken);
        var html = oembed?.GetHtml();

        return BuildMarkup(html, maxWidth, maxHeight);
    }

    /// <summary>
    /// Builds the markup by replacing dimensions in the HTML.
    /// </summary>
    /// <param name="html">The HTML from the oEmbed response.</param>
    /// <param name="maxWidth">The maximum width, or null.</param>
    /// <param name="maxHeight">The maximum height, or null.</param>
    /// <returns>The HTML with replaced dimensions, or null if input is null.</returns>
    /// <remarks>
    /// LottieFiles doesn't support maxwidth and maxheight via oEmbed,
    /// so we replace the dimensions manually using regex.
    /// Internal to support unit testing.
    /// </remarks>
    internal static string? BuildMarkup(string? html, int? maxWidth, int? maxHeight)
    {
        if (html is null)
        {
            return null;
        }

        if (maxWidth > 0 && maxHeight > 0)
        {
            html = WidthAttributeRegex().Replace(html, "width=\"" + maxWidth + "\"");
            html = HeightAttributeRegex().Replace(html, "height=\"" + maxHeight + "\"");
        }
        else
        {
            // if set to 0, let's default to 100% as an easter egg
            html = WidthAttributeRegex().Replace(html, "width=\"100%\"");
            html = HeightAttributeRegex().Replace(html, "height=\"100%\"");
        }

        return html;
    }

    [GeneratedRegex("width=\"([0-9]{1,4})\"", RegexOptions.Compiled)]
    private static partial Regex WidthAttributeRegex();

    [GeneratedRegex("height=\"([0-9]{1,4})\"", RegexOptions.Compiled)]
    private static partial Regex HeightAttributeRegex();
}
