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
        /// <remarks>Tags do not support variants.</remarks>
        public static void AssignTags(this IContentBase content, string propertyTypeAlias, IEnumerable<string> tags, bool merge = false)
        {
            content.GetTagProperty(propertyTypeAlias).AssignTags(tags, merge);
        }

        /// <summary>
        /// Remove tags.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="propertyTypeAlias">The property alias.</param>
        /// <param name="tags">The tags.</param>
        /// <remarks>Tags do not support variants.</remarks>
        public static void RemoveTags(this IContentBase content, string propertyTypeAlias, IEnumerable<string> tags)
        {
            content.GetTagProperty(propertyTypeAlias).RemoveTags(tags);
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
