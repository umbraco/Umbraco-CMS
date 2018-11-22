using System.Globalization;
using Umbraco.Core.Models;

namespace Umbraco.Core.Strings
{
    /// <summary>
    /// Default implementation of IUrlSegmentProvider.
    /// </summary>
    public class DefaultUrlSegmentProvider : IUrlSegmentProvider
    {
        /// <summary>
        /// Gets the default url segment for a specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The url segment.</returns>
        public string GetUrlSegment(IContentBase content)
        {
            return GetUrlSegmentSource(content).ToUrlSegment();
        }

        /// <summary>
        /// Gets the url segment for a specified content and culture.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The url segment.</returns>
        public string GetUrlSegment(IContentBase content, CultureInfo culture)
        {
            return GetUrlSegmentSource(content).ToUrlSegment(culture);
        }

        private static string GetUrlSegmentSource(IContentBase content)
        {
            string source = null;
            if (content.HasProperty(Constants.Conventions.Content.UrlName))
                source = (content.GetValue<string>(Constants.Conventions.Content.UrlName) ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(source))
                source = content.Name;
            return source;
        }
    }
}
