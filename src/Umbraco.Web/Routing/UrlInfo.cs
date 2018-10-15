using System.Runtime.Serialization;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Represents infos for a url.
    /// </summary>
    [DataContract(Name = "urlInfo", Namespace = "")]
    public class UrlInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlInfo"/> class.
        /// </summary>
        public UrlInfo(string text, bool isUrl, string culture)
        {
            IsUrl = isUrl;
            Text = text;
            Culture = culture;
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        [DataMember(Name = "culture")]
        public string Culture { get; }

        /// <summary>
        /// Gets a value indicating whether the url is a true url.
        /// </summary>
        /// <remarks>Otherwise, it is a message.</remarks>
        [DataMember(Name = "isUrl")]
        public bool IsUrl { get; }

        /// <summary>
        /// Gets the text, which is either the url, or a message.
        /// </summary>
        [DataMember(Name = "text")]
        public string Text { get; }

        /// <summary>
        /// Creates a <see cref="UrlInfo"/> instance representing a true url.
        /// </summary>
        public static UrlInfo Url(string text, string culture = null) => new UrlInfo(text, true, culture);

        /// <summary>
        /// Creates a <see cref="UrlInfo"/> instance representing a message.
        /// </summary>
        public static UrlInfo Message(string text, string culture = null) => new UrlInfo(text, false, culture);
    }
}
