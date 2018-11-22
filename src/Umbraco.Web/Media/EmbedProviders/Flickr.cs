using System;
using System.ComponentModel;
using System.Web;

namespace Umbraco.Web.Media.EmbedProviders
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This is no longer used and will be removed from the codebase in the future, for Flickr, use the Umbraco.Web.Media.EmbedProviders.OEmbedPhoto provider")]
    public class Flickr : AbstractOEmbedProvider
    {
        public override string GetMarkup(string url, int maxWidth, int maxHeight)
        {
            var flickrUrl = BuildFullUrl(url, maxWidth, maxHeight);
            var doc = GetXmlResponse(flickrUrl);

            string imageUrl = doc.SelectSingleNode("/oembed/url").InnerText;
            string imageWidth = doc.SelectSingleNode("/oembed/width").InnerText;
            string imageHeight = doc.SelectSingleNode("/oembed/height").InnerText;
            string imageTitle = doc.SelectSingleNode("/oembed/title").InnerText;

            return string.Format("<img src=\"{0}\" width\"{1}\" height=\"{2}\" alt=\"{3}\" />",
                imageUrl, imageWidth, imageHeight, HttpUtility.HtmlEncode(imageTitle));
        }
    }
}