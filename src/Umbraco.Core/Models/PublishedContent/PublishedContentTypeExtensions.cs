using System;

namespace Umbraco.Core.Models.PublishedContent
{
    public static class PublishedContentTypeExtensions
    {
        /// <summary>
        /// Get the GUID key from an <see cref="IPublishedContentType"/>
        /// </summary>
        /// <param name="publishedContentType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool TryGetKey(this IPublishedContentType publishedContentType, out Guid key)
        {
            if (publishedContentType is IPublishedContentType2 contentTypeWithKey)
            {
                key = contentTypeWithKey.Key;
                return true;
            }
            key = Guid.Empty;
            return false;                
        }
    }
}
