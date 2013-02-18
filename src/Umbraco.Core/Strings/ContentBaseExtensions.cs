using System;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Strings
{
    /// <summary>
    /// Provides extension methods to IContentBase to get url segments.
    /// </summary>
    internal static class ContentBaseExtensions
    {
        /// <summary>
        /// Gets the default url segment for a specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The url segment.</returns>
        public static string GetUrlSegment(this IContentBase content)
        {
            var urlSegmentProviders = UrlSegmentProviderResolver.Current.Providers;
            var url = urlSegmentProviders.Select(p => p.GetUrlSegment(content)).First(u => u != null);
            url = url ?? new DefaultUrlSegmentProvider().GetUrlSegment(content); // be safe
            return url;
        }

        /// <summary>
        /// Gets the url segment for a specified content and culture.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The url segment.</returns>
        public static string GetUrlSegment(this IContentBase content, CultureInfo culture)
        {
            var urlSegmentProviders = UrlSegmentProviderResolver.Current.Providers;
            var url = urlSegmentProviders.Select(p => p.GetUrlSegment(content, culture)).First(u => u != null);
            url = url ?? new DefaultUrlSegmentProvider().GetUrlSegment(content, culture); // be safe
            return url;
        }
    }
}
