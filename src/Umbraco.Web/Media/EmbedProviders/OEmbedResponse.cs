using System.Text;
using System.Web;
using Newtonsoft.Json;

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

        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("author_url")]
        public string AuthorUrl { get; set; }

        [JsonProperty("provider_name")]
        public string ProviderName { get; set; }

        [JsonProperty("provider_url")]
        public string ProviderUrl { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("thumbnail_height")]
        public int? ThumbnailHeight { get; set; }

        [JsonProperty("thumbnail_width")]
        public int? ThumbnailWidth { get; set; }

        public string Html { get; set; }

        public string Url { get; set; }

        public int? Height { get; set; }

        public int? Width { get; set; }

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
