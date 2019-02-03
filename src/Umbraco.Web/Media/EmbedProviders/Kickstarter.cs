using System.Collections.Generic;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class Kickstarter : EmbedProviderBase
    {
        public override string ApiEndpoint => "http://www.kickstarter.com/services/oembed";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"kickstarter\.com/projects/*"
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
