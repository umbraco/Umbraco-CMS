using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Embed Provider for Twitter the popular online service for microblogging and social networking.
/// </summary>
[Obsolete("Please use X instead, scheduled for removal in v16")]
public class Twitter : OEmbedProviderBase
{
    public Twitter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://publish.twitter.com/oembed";

    public override string[] UrlSchemeRegex => new[] { @"twitter.com/.*/status/.*" };

    public override Dictionary<string, string> RequestParams => new();

    [Obsolete("Use GetOEmbedDataAsync instead. This will be removed in Umbraco 15.")]
    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        return GetOEmbedDataAsync(url, maxWidth, maxHeight, CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<string?> GetOEmbedDataAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse? oembed = await base.GetJsonResponseAsync<OEmbedResponse>(requestUrl, cancellationToken);

        return oembed?.GetHtml();
    }
}
