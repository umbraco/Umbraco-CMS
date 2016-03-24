using System;
using System.Collections.Generic;
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
        /// <param name="urlSegmentProviders"></param>
        /// <returns>The url segment.</returns>
        public static string GetUrlSegment(this IContentBase content, IEnumerable<IUrlSegmentProvider> urlSegmentProviders)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (urlSegmentProviders == null) throw new ArgumentNullException("urlSegmentProviders");

            var url = urlSegmentProviders.Select(p => p.GetUrlSegment(content)).FirstOrDefault(u => u != null);
            url = url ?? new DefaultUrlSegmentProvider().GetUrlSegment(content); // be safe
            return url;
        }

        /// <summary>
        /// Gets the url segment for a specified content and culture.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="urlSegmentProviders"></param>
        /// <returns>The url segment.</returns>
        public static string GetUrlSegment(this IContentBase content, CultureInfo culture, IEnumerable<IUrlSegmentProvider> urlSegmentProviders)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (culture == null) throw new ArgumentNullException("culture");
            if (urlSegmentProviders == null) throw new ArgumentNullException("urlSegmentProviders");

            var url = urlSegmentProviders.Select(p => p.GetUrlSegment(content, culture)).FirstOrDefault(u => u != null);
            url = url ?? new DefaultUrlSegmentProvider().GetUrlSegment(content, culture); // be safe
            return url;
        }
    }
}
