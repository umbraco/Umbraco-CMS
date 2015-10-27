using System.Web;
using System.Xml;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class OEmbedPhoto : AbstractOEmbedProvider
    {
        public override string GetMarkup(string url, int maxWidth, int maxHeight)
        {
            string requestUrl = BuildFullUrl(url, maxWidth, maxHeight);

            XmlDocument doc = GetXmlResponse(requestUrl);
            string imageUrl = GetXmlProperty(doc, "/oembed/url");
            string imageWidth = GetXmlProperty(doc, "/oembed/width");
            string imageHeight = GetXmlProperty(doc, "/oembed/height");
            string imageTitle = GetXmlProperty(doc, "/oembed/title");

            return string.Format("<img src=\"{0}\" width\"{1}\" height=\"{2}\" alt=\"{3}\" />",
                imageUrl, imageWidth, imageHeight, HttpUtility.HtmlEncode(imageTitle));
        }
    }
}