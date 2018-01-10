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
            if (type == typeof(IContentType)) itemType = PublishedItemType.Content;
            else if (type == typeof(IMediaType)) itemType = PublishedItemType.Media;
            else if (type == typeof(IMemberType)) itemType = PublishedItemType.Member;
            return itemType;
        }
    }
}
