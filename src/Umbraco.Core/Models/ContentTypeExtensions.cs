using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    //fixme: This whole thing needs to go, it's super hacky and doens't need to exist in the first place
    internal static class ContentTypeExtensions
    {
        /// <summary>
        /// Get all descendant content types
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentTypeService"></param>
        /// <returns></returns>
        public static IEnumerable<IContentTypeBase> Descendants(this IContentTypeBase contentType, IContentTypeService contentTypeService)
        {
            if (contentType is IContentType)
            {
                var descendants = contentTypeService.GetContentTypeChildren(contentType.Id)
                    .SelectRecursive(type => contentTypeService.GetContentTypeChildren(type.Id));
                return descendants;
            }

            if (contentType is IMediaType)
            {
                var descendants = contentTypeService.GetMediaTypeChildren(contentType.Id)
                    .SelectRecursive(type => contentTypeService.GetMediaTypeChildren(type.Id));
                return descendants;
            }

            //No other content types have children (i.e. member types)
            return Enumerable.Empty<IContentTypeBase>();
        }

        /// <summary>
        /// Get all descendant content types
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentTypeService"></param>
        /// <returns></returns>
        public static IEnumerable<IContentTypeBase> Descendants(this IContentTypeBase contentType, ContentTypeServiceBase contentTypeService)
        {
            var cService = contentTypeService as IContentTypeService;

            return cService == null ? Enumerable.Empty<IContentTypeBase>() : contentType.Descendants(cService);
        }

        /// <summary>
        /// Get all descendant and self content types
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentTypeService"></param>
        /// <returns></returns>
        public static IEnumerable<IContentTypeBase> DescendantsAndSelf(this IContentTypeBase contentType, IContentTypeService contentTypeService)
        {
            var descendantsAndSelf = new[] { contentType }.Concat(contentType.Descendants(contentTypeService));
            return descendantsAndSelf;
        }

        /// <summary>
        /// Get all descendant and self content types
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="contentTypeService"></param>
        /// <returns></returns>
        public static IEnumerable<IContentTypeBase> DescendantsAndSelf(this IContentTypeBase contentType, ContentTypeServiceBase contentTypeService)
        {
            var descendantsAndSelf = new[] { contentType }.Concat(contentType.Descendants(contentTypeService));
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