using System.Collections.Generic;
using System.Web;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class Flickr : EmbedProviderBase
    {
        public override string ApiEndpoint => "http://www.flickr.com/services/oembed/";

        public override string[] UrlSchemeRegex => new string[]
        {
            @"flickr.com\/photos\/*",
            @"flic.kr\/p\/*"
        };

        public override Dictionary<string, string> RequestParams => new Dictionary<string, string>();
                
        public override string GetMarkup(string url, int maxWidth = 0, int maxHeight = 0)
        {
            var requestUrl = base.GetEmbedProviderUrl(url, maxWidth, maxHeight);
            var xmlDocument = base.GetXmlResponse(requestUrl);

            var imageUrl = GetXmlProperty(xmlDocument, "/oembed/url");
            var imageWidth = GetXmlProperty(xmlDocument, "/oembed/width");
            var imageHeight = GetXmlProperty(xmlDocument, "/oembed/height");
            var imageTitle = GetXmlProperty(xmlDocument, "/oembed/title");

            return string.Format("<img src=\"{0}\" width\"{1}\" height=\"{2}\" alt=\"{3}\" />", imageUrl, imageWidth, imageHeight, HttpUtility.HtmlEncode(imageTitle));
        }        
    }
}
