using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models
{
    public static class ContentTypeExtensions
    {
        /// <summary>
        /// Get all descendant content types
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IEnumerable<IContentType> Descendants(this IContentType contentType)
        {
            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
            var descendants = contentTypeService.GetContentTypeChildren(contentType.Id)
                                                .FlattenList(type => contentTypeService.GetContentTypeChildren(type.Id));
            return descendants;
        }

        /// <summary>
        /// Get all descendant and self content types
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IEnumerable<IContentType> DescendantsAndSelf(this IContentType contentType)
        {
            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
            var descendants = contentTypeService.GetContentTypeChildren(contentType.Id)
                                                .FlattenList(type => contentTypeService.GetContentTypeChildren(type.Id));
            var descendantsAndSelf = new[] { contentType }.Concat(contentType.Descendants());
            return descendantsAndSelf;
        }
    }
}