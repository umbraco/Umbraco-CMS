using System;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web
{
    public static class PublishedContentQueryExtensions
    {
        /// <summary>
        /// Gets a content item from the cache
        /// </summary>
        /// <param name="contentQuery"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IPublishedContent TypedContent(this ITypedPublishedContentQuery contentQuery, Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new InvalidOperationException("UDIs for content items must be " + typeof(GuidUdi));
            return contentQuery.TypedContent(guidUdi.Guid);
        }
    }
}