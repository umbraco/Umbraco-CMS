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
    }
}
