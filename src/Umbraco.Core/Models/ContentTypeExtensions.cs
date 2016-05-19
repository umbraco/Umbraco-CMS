using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    internal static class ContentTypeExtensions
    {
        /// <summary>
        /// Get all descendant content types
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentTypeService"></param>
        /// <returns></returns>
        public static IEnumerable<TItem> Descendants<TItem>(this TItem contentType, IContentTypeServiceBase<TItem> contentTypeService) 
            where TItem : IContentTypeComposition
        {            
            var descendants = contentTypeService.GetChildren(contentType.Id)
                                                .SelectRecursive(type => contentTypeService.GetChildren(type.Id));
            return descendants;
        }

        /// <summary>
        /// Get all descendant and self content types
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentTypeService"></param>
        /// <returns></returns>
        public static IEnumerable<TItem> DescendantsAndSelf<TItem>(this TItem contentType, IContentTypeServiceBase<TItem> contentTypeService) 
            where TItem : IContentTypeComposition
        {
            var descendantsAndSelf = new[] { contentType }.Concat(contentType.Descendants<TItem>(contentTypeService));
            return descendantsAndSelf;
        }
        
    }
}