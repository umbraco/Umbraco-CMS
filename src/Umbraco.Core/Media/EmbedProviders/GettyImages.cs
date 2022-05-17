using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

// TODO(V10) : change base class to OEmbedProviderBase
public class GettyImages : EmbedProviderBase
{
    public GettyImages(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    public override string ApiEndpoint => "http://embed.gettyimages.com/oembed";

    // http://gty.im/74917285
    // http://www.gettyimages.com/detail/74917285
    public override string[] UrlSchemeRegex => new[] { @"gty\.im/*", @"gettyimages.com\/detail\/*" };

    public override Dictionary<string, string> RequestParams => new();

    public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
    {
        var requestUrl = this.GetEmbedProviderUrl(url, maxWidth, maxHeight);
        OEmbedResponse? oembed = this.GetJsonResponse<OEmbedResponse>(requestUrl);

        return oembed?.GetHtml();
    }
}
