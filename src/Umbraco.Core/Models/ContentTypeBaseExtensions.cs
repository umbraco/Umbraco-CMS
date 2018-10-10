using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Provides extensions methods for <see cref="IContentTypeBase"/>.
    /// </summary>
    public static class ContentTypeBaseExtensions
    {
        public static PublishedItemType GetItemType(this IContentTypeBase contentType)
        {
            var type = contentType.GetType();
            var itemType = PublishedItemType.Unknown;
            if (typeof(IContentType).IsAssignableFrom(type)) itemType = PublishedItemType.Content;
            else if (typeof(IMediaType).IsAssignableFrom(type)) itemType = PublishedItemType.Media;
            else if (typeof(IMemberType).IsAssignableFrom(type)) itemType = PublishedItemType.Member;
            return itemType;
        }

        /// <summary>
        /// Used to check if any property type was changed between variant/invariant
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        internal static bool WasPropertyTypeVariationChanged(this IContentTypeBase contentType)
        {
            return contentType.WasPropertyTypeVariationChanged(out var _);
        }

        /// <summary>
        /// Used to check if any property type was changed between variant/invariant
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        internal static bool WasPropertyTypeVariationChanged(this IContentTypeBase contentType, out IReadOnlyCollection<string> aliases)
        {
            var a = new List<string>();

            // property variation change?
            var hasAnyPropertyVariationChanged = contentType.PropertyTypes.Any(propertyType =>
            {
                if (!(propertyType is IRememberBeingDirty dirtyProperty))
                    throw new Exception("oops");

                // skip new properties
                //TODO: This used to be WasPropertyDirty("HasIdentity") but i don't think that actually worked for detecting new entities this does seem to work properly
                var isNewProperty = dirtyProperty.WasPropertyDirty("Id");
                if (isNewProperty) return false;

                // variation change?
                var dirty = dirtyProperty.WasPropertyDirty("Variations");
                if (dirty)
                    a.Add(propertyType.Alias);

                return dirty;

            });

            aliases = a;
            return hasAnyPropertyVariationChanged;
        }

        /// <summary>
        /// Returns the list of content types the composition is used in
        /// </summary>
        /// <param name="allContentTypes"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static IEnumerable<IContentTypeComposition> GetWhereCompositionIsUsedInContentTypes(this IContentTypeComposition source,
            IContentTypeComposition[] allContentTypes)
        {
            var sourceId = source != null ? source.Id : 0;

            // find which content types are using this composition
            return allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == sourceId)).ToArray();
        }
    }
}
