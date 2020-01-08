using System.Collections.Generic;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class DailyMotion : EmbedProviderBase
    {
        public override string ApiEndpoint => "https://www.dailymotion.com/services/oembed";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"dailymotion.com/video/.*"
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
