using System.Xml;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class OEmbedVideo : AbstractOEmbedProvider
    {
        public override string GetMarkup(string url, int maxWidth, int maxHeight)
        {
            string requestUrl = BuildFullUrl(url, maxWidth, maxHeight);

            XmlDocument doc = GetXmlResponse(requestUrl);
            return GetXmlProperty(doc, "/oembed/html");
        }
    }
}