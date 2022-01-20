using System.Collections.Generic;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders
{
    // TODO(V10) : change base class to OEmbedProviderBase
    public class Soundcloud : EmbedProviderBase
    {
        public override string ApiEndpoint => "https://soundcloud.com/oembed";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"soundcloud.com\/*"
        };

        public override Dictionary<string, string> RequestParams => new Dictionary<string, string>();

        public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        {
            var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
            var xmlDocument = base.GetXmlResponse(requestUrl);

            return GetXmlProperty(xmlDocument, "/oembed/html");
        }

        public Soundcloud(IJsonSerializer jsonSerializer) : base(jsonSerializer)
        {
        }
    }
}
