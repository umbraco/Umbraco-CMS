﻿using Umbraco.Core.Models;

namespace Umbraco.Core.Strings
{
    /// <summary>
    /// Default implementation of IUrlSegmentProvider.
    /// </summary>
    public class DefaultUrlSegmentProvider : IUrlSegmentProvider
    {
        /// <summary>
        /// Gets the URL segment for a specified content and culture.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The URL segment.</returns>
        public string GetUrlSegment(IContentBase content, string culture = null)
        {
            return GetUrlSegmentSource(content, culture).ToUrlSegment(culture);
        }

        private static string GetUrlSegmentSource(IContentBase content, string culture)
        {
            string source = null;
            if (content.HasProperty(Constants.Conventions.Content.UrlName))
                source = (content.GetValue<string>(Constants.Conventions.Content.UrlName, culture) ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(source))
            {
                // If the name of a node has been updated, but it has not been published, the url should use the published name, not the current node name
                source = (content is IContent document) && document.Edited
                    ? document.GetPublishName(culture)
                    : content.GetCultureName(culture);
            }
            return source;
        }
    }
}
