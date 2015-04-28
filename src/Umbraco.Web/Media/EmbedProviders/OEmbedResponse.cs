using System.Web;

namespace Umbraco.Web.Media.EmbedProviders
{
    /// <summary>
    /// Wrapper class for OEmbed response
    /// </summary>
    public class OEmbedResponse
    {
        public string Type { get; set; }

        public string Version { get; set; }

        public string Title { get; set; }

        public string AuthorName { get; set; }

        public string AuthorUrl { get; set; }

        public string ProviderName { get; set; }

        public string ProviderUrl { get; set; }

        public string ThumbnailUrl { get; set; }

        public string ThumbnailHeight { get; set; }

        public string ThumbnailWidth { get; set; }

        public string Html { get; set; }

        public string Url { get; set; }

        public string Height { get; set; }

        public string Width { get; set; }

        /// <summary>
        /// Gets the HTML.
        /// </summary>
        /// <returns>The response html</returns>
        public string GetHtml()
        {
            if (Type == "photo")
            {
                return "<img src=\"" + Url + "\" width=\"" + Width + "\" height=\"" + Height + "\" alt=\"" + HttpUtility.HtmlEncode(Title) + "\" />";
            }

            return string.IsNullOrEmpty(Html) == false ? Html : string.Empty;
        }
    }
}
