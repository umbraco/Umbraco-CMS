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
        /// <returns></returns>
        public static IEnumerable<IContentTypeBase> Descendants(this IContentTypeBase contentType)
        {
            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
            var descendants = contentTypeService.GetContentTypeChildren(contentType.Id)
                                                .SelectRecursive(type => contentTypeService.GetContentTypeChildren(type.Id));
            return descendants;
        }

        /// <summary>
        /// Get all descendant and self content types
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IEnumerable<IContentTypeBase> DescendantsAndSelf(this IContentTypeBase contentType)
        {
            var descendantsAndSelf = new[] { contentType }.Concat(contentType.Descendants());
            return descendantsAndSelf;
        }

        ///// <summary>
        ///// Returns the descendant content type Ids for the given content type
        ///// </summary>
        ///// <param name="contentType"></param>
        ///// <returns></returns>
        //public static IEnumerable<int> DescendantIds(this IContentTypeBase contentType)
        //{
        //    return ((ContentTypeService) ApplicationContext.Current.Services.ContentTypeService)
        //        .GetDescendantContentTypeIds(contentType.Id);
        //}
    }
}