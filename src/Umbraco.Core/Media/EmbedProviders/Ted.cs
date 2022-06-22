using System.Collections.Generic;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders
{
    // TODO(V10) : change base class to OEmbedProviderBase
    public class Ted : EmbedProviderBase
    {
        public override string ApiEndpoint => "http://www.ted.com/talks/oembed.xml";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"ted.com\/talks\/*"
        };

        public override Dictionary<string, string> RequestParams => new Dictionary<string, string>();

        public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        {
            var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
            var xmlDocument = base.GetXmlResponse(requestUrl);

            return GetXmlProperty(xmlDocument, "/oembed/html");
        }

        public Ted(IJsonSerializer jsonSerializer) : base(jsonSerializer)
        {
        }
    }
}
