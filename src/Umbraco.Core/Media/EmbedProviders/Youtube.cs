using System.Collections.Generic;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders
{
    // TODO(V10) : change base class to OEmbedProviderBase
    public class YouTube : EmbedProviderBase
    {
        public override string ApiEndpoint => "https://www.youtube.com/oembed";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"youtu.be/.*",
            @"youtube.com/watch.*"
        };

        public override Dictionary<string, string> RequestParams => new Dictionary<string, string>()
        {
            //ApiUrl/?format=json
            {"format", "json"}
        };

        public override string? GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        {
            var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
            var oembed = base.GetJsonResponse<OEmbedResponse>(requestUrl);

            return oembed?.GetHtml();
        }

        public YouTube(IJsonSerializer jsonSerializer) : base(jsonSerializer)
        {
        }
    }
}
