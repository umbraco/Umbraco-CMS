using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    internal static class ContentTypeExtensions
    {
        /// <summary>
        /// Gets all descendant content types of a specified content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="contentTypeService">The content type service.</param>
        /// <returns>The descendant content types.</returns>
        /// <remarks>Descendants corresponds to the parent-child relationship, and has
        /// nothing to do with compositions, though a child should always be composed
        /// of its parent.</remarks>
        public static IEnumerable<TItem> Descendants<TItem>(this TItem contentType, IContentTypeServiceBase<TItem> contentTypeService)
            where TItem : IContentTypeComposition
        {
            return contentTypeService.GetDescendants(contentType.Id, false);
        }

        /// <summary>
        /// Gets all descendant and self content types of a specified content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="contentTypeService">The content type service.</param>
        /// <returns>The descendant and self content types.</returns>
        /// <remarks>Descendants corresponds to the parent-child relationship, and has
        /// nothing to do with compositions, though a child should always be composed
        /// of its parent.</remarks>
        public static IEnumerable<TItem> DescendantsAndSelf<TItem>(this TItem contentType, IContentTypeServiceBase<TItem> contentTypeService)
            where TItem : IContentTypeComposition
        {
            return contentTypeService.GetDescendants(contentType.Id, true);
        }

        /// <summary>
        /// Gets all content types directly or indirectly composed of a specified content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <param name="contentTypeService">The content type service.</param>
        /// <returns>The content types directly or indirectly composed of the content type.</returns>
        /// <remarks>This corresponds to the composition relationship and has nothing to do
        /// with the parent-child relationship, though a child should always be composed of
        /// its parent.</remarks>
        public static IEnumerable<TItem> ComposedOf<TItem>(this TItem contentType, IContentTypeServiceBase<TItem> contentTypeService)
            where TItem : IContentTypeComposition
        {
            return contentTypeService.GetComposedOf(contentType.Id);
        }
    }
}