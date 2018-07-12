using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // what's needed to actually build a content node
    internal struct ContentNodeKit
    {
        public ContentNode Node;
        public int ContentTypeId;
        public ContentData DraftData;
        public ContentData PublishedData;

        public bool IsEmpty => Node == null;

        public bool IsNull => ContentTypeId < 0;

        public static ContentNodeKit Null { get; } = new ContentNodeKit { ContentTypeId = -1 };

        public void Build(PublishedContentType contentType, IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor, bool canBePublished)
        {
            Node.SetContentTypeAndData(contentType, DraftData, canBePublished ? PublishedData : null, publishedSnapshotAccessor, variationContextAccessor);
        }
    }
}
