using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders
{
    public class DailyMotion : OEmbedProviderBase
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

        public DailyMotion(IJsonSerializer jsonSerializer) : base(jsonSerializer)
        {
        }
    }
}
