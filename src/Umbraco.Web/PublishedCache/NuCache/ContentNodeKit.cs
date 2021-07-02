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

        public static ContentNodeKit Empty { get; } = new ContentNodeKit();
        public static ContentNodeKit Null { get; } = new ContentNodeKit { ContentTypeId = -1 };

        public void Build(
            IPublishedContentType contentType,
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IVariationContextAccessor variationContextAccessor,
            bool canBePublished)
        {
            var draftData = DraftData;

            // no published data if it cannot be published (eg is masked)
            var publishedData = canBePublished ? PublishedData : null;

            // we *must* have either published or draft data
            // if it cannot be published, published data is going to be null
            // therefore, ensure that draft data is not
            if (draftData == null && !canBePublished)
                draftData = PublishedData;

            Node.SetContentTypeAndData(contentType, draftData, publishedData, publishedSnapshotAccessor, variationContextAccessor);
        }

        public ContentNodeKit Clone()
            => new ContentNodeKit
            {
                ContentTypeId = ContentTypeId,
                DraftData = DraftData,
                PublishedData = PublishedData,
                Node = new ContentNode(Node)
            };
    }
}
