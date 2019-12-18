using System.Collections.Generic;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class Issuu : EmbedProviderBase
    {
        public override string ApiEndpoint => "https://issuu.com/oembed";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"issuu.com/.*/docs/.*"
        };
        
        public override Dictionary<string, string> RequestParams => new Dictionary<string, string>()
        {
            //ApiUrl/?format=xml
            {"format", "xml"}
        };

        public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        {
            var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
            var xmlDocument = base.GetXmlResponse(requestUrl);

            return GetXmlProperty(xmlDocument, "/oembed/html");
        }
    }
}
