using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Strings
{
    /// <summary>
    /// Provides extension methods to IContentBase to get URL segments.
    /// </summary>
    public static class ContentBaseExtensions
    {
        /// <summary>
        /// Gets the URL segment for a specified content and culture.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="shortStringHelper"></param>
        /// <param name="urlSegmentProviders"></param>
        /// <param name="culture">The culture.</param>
        /// <returns>The URL segment.</returns>
        public static string GetUrlSegment(this IContentBase content, IShortStringHelper shortStringHelper, IEnumerable<IUrlSegmentProvider> urlSegmentProviders, string culture = null)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (urlSegmentProviders == null) throw new ArgumentNullException(nameof(urlSegmentProviders));

            var url = urlSegmentProviders.Select(p => p.GetUrlSegment(content, culture)).FirstOrDefault(u => u != null);
            url ??= new DefaultUrlSegmentProvider(shortStringHelper).GetUrlSegment(content, culture); // be safe
            return url;
        }
    }
}
