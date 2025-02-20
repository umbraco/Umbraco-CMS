using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Kickstarter the popular online crowdfunding platform focused on creativity.
/// </summary>
public class Kickstarter : OEmbedProviderBase
{
    public Kickstarter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://www.kickstarter.com/services/oembed";

    public override string[] UrlSchemeRegex => new[] { @"kickstarter\.com/projects/*" };

    public override Dictionary<string, string> RequestParams => new();

    [Obsolete("Use GetMarkupAsync instead. This will be removed in Umbraco 15.")]
    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        => GetMarkupAsync(url, maxWidth, maxHeight, CancellationToken.None).GetAwaiter().GetResult();

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);

    [Obsolete("Use GetMarkupAsync instead. Planned for removal in v16")]
    public override async Task<string?> GeOEmbedDataAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse? oembed = await base.GetJsonResponseAsync<OEmbedResponse>(requestUrl, cancellationToken);

        return oembed?.GetHtml();
    }
}
