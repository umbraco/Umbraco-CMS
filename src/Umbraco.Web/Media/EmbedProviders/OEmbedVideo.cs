using System.Xml;

namespace Umbraco.Web.Media.EmbedProviders
{
    public class OEmbedVideo : AbstractOEmbedProvider
    {
        public override string GetMarkup(string url, int maxWidth, int maxHeight)
        {
            string videoUrl = BuildFullUrl(url, maxWidth, maxHeight) ;
           
            XmlDocument doc = GetXmlResponse(videoUrl);
           
            // add xslt transformation to return markup
            return doc.SelectSingleNode("/oembed/html").InnerText;
        }
    }
}