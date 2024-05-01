using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for lottiefiles.com the popular opensource JSON-based animation format platform.
/// </summary>
public class LottieFiles : OEmbedProviderBase
{
    public LottieFiles(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "https://embed.lottiefiles.com/oembed";

    public override string[] UrlSchemeRegex => new[] { @"lottiefiles\.com/*" };

    public override Dictionary<string, string> RequestParams => new();

    [Obsolete("Use GetMarkupAsync instead. This will be removed in Umbraco 15.")]
    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        return GeOEmbedDataAsync(url, maxWidth, maxHeight, CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<string?> GeOEmbedDataAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = this.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse? oembed = await this.GetJsonResponseAsync<OEmbedResponse>(requestUrl, cancellationToken);
        var html = oembed?.GetHtml();

        // LottieFiles doesn't seem to support maxwidth and maxheight via oembed
        // this is therefore a hack... with regexes.. is that ok? HtmlAgility etc etc
        // otherwise it always defaults to 300...
        if (html is null)
        {
            return null;
        }

        if (maxWidth > 0 && maxHeight > 0)
        {
            html = Regex.Replace(html, "width=\"([0-9]{1,4})\"", "width=\"" + maxWidth + "\"");
            html = Regex.Replace(html, "height=\"([0-9]{1,4})\"", "height=\"" + maxHeight + "\"");
        }
        else
        {
            // if set to 0, let's default to 100% as an easter egg
            html = Regex.Replace(html, "width=\"([0-9]{1,4})\"", "width=\"100%\"");
            html = Regex.Replace(html, "height=\"([0-9]{1,4})\"", "height=\"100%\"");
        }

        return html;
    }
}
