﻿using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Strings
{
    /// <summary>
    /// Default implementation of IUrlSegmentProvider.
    /// </summary>
    public class DefaultUrlSegmentProvider : IUrlSegmentProvider
    {
        private readonly IShortStringHelper _shortStringHelper;

        public DefaultUrlSegmentProvider(IShortStringHelper shortStringHelper)
        {
            _shortStringHelper = shortStringHelper;
        }

        /// <summary>
        /// Gets the URL segment for a specified content and culture.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The URL segment.</returns>
        public string GetUrlSegment(IContentBase content, string culture = null)
        {
            return GetUrlSegmentSource(content, culture).ToUrlSegment(_shortStringHelper, culture);
        }

        private static string GetUrlSegmentSource(IContentBase content, string culture)
        {
            string source = null;
            if (content.HasProperty(Constants.Conventions.Content.UrlName))
                source = (content.GetValue<string>(Constants.Conventions.Content.UrlName, culture) ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(source))
                source = content.GetCultureName(culture);
            return source;
        }
    }
}
