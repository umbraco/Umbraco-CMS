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

    public override async Task<string?> GetMarkupAsync(string url, int? maxWidth, int? maxHeight, CancellationToken cancellationToken)
        => await GetJsonBasedMarkupAsync(url, maxWidth, maxHeight, cancellationToken);
}
