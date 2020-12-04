using System.Net;
using System.Runtime.Serialization;

namespace Umbraco.Web.Media.EmbedProviders
{
    /// <summary>
    /// Wrapper class for OEmbed response
    /// </summary>
    [DataContract]
    public class OEmbedResponse
    {
        public string Type { get; set; }

        public string Version { get; set; }

        public string Title { get; set; }

        [DataMember(Name ="author_name")]
        public string AuthorName { get; set; }

        [DataMember(Name ="author_url")]
        public string AuthorUrl { get; set; }

        [DataMember(Name ="provider_name")]
        public string ProviderName { get; set; }

        [DataMember(Name ="provider_url")]
        public string ProviderUrl { get; set; }

        [DataMember(Name ="thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [DataMember(Name ="thumbnail_height")]
        public double? ThumbnailHeight { get; set; }

        [DataMember(Name ="thumbnail_width")]
        public double? ThumbnailWidth { get; set; }

        public string Html { get; set; }

        public string Url { get; set; }

        public double? Height { get; set; }

        public double? Width { get; set; }

        /// <summary>
        /// Gets the HTML.
        /// </summary>
        /// <returns>The response HTML</returns>
        public string GetHtml()
        {
            if (Type == "photo")
            {
                return "<img src=\"" + Url + "\" width=\"" + Width + "\" height=\"" + Height + "\" alt=\"" + WebUtility.HtmlEncode(Title) + "\" />";
            }

            return string.IsNullOrEmpty(Html) == false ? Html : string.Empty;
        }
    }
}
