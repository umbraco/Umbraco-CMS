using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="IContentBase"/> class, to manage tags.
    /// </summary>
    public static class ContentTagsExtensions
    {
        /// <summary>
        /// Assign tags.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="merge">A value indicating whether to merge the tags with existing tags instead of replacing them.</param>
        /// <param name="culture">A culture, for multi-lingual properties.</param>
        public static void AssignTags(this IContentBase content, string propertyTypeAlias, IEnumerable<string> tags, bool merge = false, string culture = null)
        {
            content.GetTagProperty(propertyTypeAlias).AssignTags(tags, merge, culture);
        }

        /// <summary>
        /// Remove tags.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="culture">A culture, for multi-lingual properties.</param>
        public static void RemoveTags(this IContentBase content, string propertyTypeAlias, IEnumerable<string> tags, string culture = null)
        {
            content.GetTagProperty(propertyTypeAlias).RemoveTags(tags, culture);
        }

        // gets and validates the property
        private static Property GetTagProperty(this IContentBase content, string propertyTypeAlias)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var property = content.Properties[propertyTypeAlias];
            if (property != null) return property;

            throw new IndexOutOfRangeException($"Could not find a property with alias \"{propertyTypeAlias}\".");
        }
    }
}
