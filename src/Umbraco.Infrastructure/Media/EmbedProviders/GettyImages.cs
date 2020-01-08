using System.Collections.Generic;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class GettyImages : EmbedProviderBase
    {
        public override string ApiEndpoint => "http://embed.gettyimages.com/oembed";

        //http://gty.im/74917285
        //http://www.gettyimages.com/detail/74917285
        public override string[] UrlSchemeRegex => new string[]
        {
            @"gty\.im/*",
            @"gettyimages.com\/detail\/*"
        };

        public override Dictionary<string, string> RequestParams => new Dictionary<string, string>();

        public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        {
            var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
            var oembed = base.GetJsonResponse<OEmbedResponse>(requestUrl);

            return oembed.GetHtml();
        }
    }
}
