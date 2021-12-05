using System.Collections.Generic;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders
{
    /// <summary>
    /// Embed Provider for Giphy.com the popular online GIFs and animated sticker provider.
    /// </summary>
    /// TODO(V10) : change base class to OEmbedProviderBase
    public class Giphy : EmbedProviderBase
    {
        public override string ApiEndpoint => "https://giphy.com/services/oembed?url=";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"giphy\.com/*",
            @"gph\.is/*"
        };

        public override Dictionary<string, string> RequestParams => new Dictionary<string, string>();

        public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        {
            var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
            var oembed = base.GetJsonResponse<OEmbedResponse>(requestUrl);

            return oembed.GetHtml();
        }

        public Giphy(IJsonSerializer jsonSerializer) : base(jsonSerializer)
        {
        }
    }
}
