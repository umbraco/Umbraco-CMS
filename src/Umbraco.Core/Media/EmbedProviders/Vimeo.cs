using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders
{
    public class Vimeo : OEmbedProviderBase
    {
        public override string ApiEndpoint => "https://vimeo.com/api/oembed.xml";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"vimeo\.com/"
        };

        public override Dictionary<string, string> RequestParams => new Dictionary<string, string>();

        public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        {
            var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
            var xmlDocument = base.GetXmlResponse(requestUrl);

            return GetXmlProperty(xmlDocument, "/oembed/html");
        }

        public Vimeo(IJsonSerializer jsonSerializer) : base(jsonSerializer)
        {
        }
    }
}
