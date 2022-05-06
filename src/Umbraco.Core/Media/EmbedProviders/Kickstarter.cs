using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

// TODO(V10) : change base class to OEmbedProviderBase
public class Kickstarter : EmbedProviderBase
{
    public Kickstarter(IJsonSerializer jsonSerializer) : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://www.kickstarter.com/services/oembed";

    public override string[] UrlSchemeRegex => new[] {@"kickstarter\.com/projects/*"};

    public override Dictionary<string, string> RequestParams => new();

    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse oembed = base.GetJsonResponse<OEmbedResponse>(requestUrl);

        return oembed?.GetHtml();
    }
}
