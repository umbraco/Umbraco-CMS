using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

public struct ContentNodeKit
{
    public ContentNode Node { get; } = null!;

    public int ContentTypeId { get; }

    public ContentData? DraftData { get; }

    public ContentData? PublishedData { get; }

    public ContentNodeKit(ContentNode node, int contentTypeId, ContentData? draftData, ContentData? publishedData)
    {
        Node = node;
        ContentTypeId = contentTypeId;
        DraftData = draftData;
        PublishedData = publishedData;
    }

    public static ContentNodeKit Empty { get; } = default(ContentNodeKit);

    public bool IsEmpty => Node == null;

    public bool IsNull => ContentTypeId < 0;

    public static ContentNodeKit Null { get; } = new(null!, -1, null, null);

    public void Build(
        IPublishedContentType contentType,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IVariationContextAccessor variationContextAccessor,
        IPublishedModelFactory publishedModelFactory,
        bool canBePublished)
    {
        ContentData? draftData = DraftData;

        // no published data if it cannot be published (eg is masked)
        ContentData? publishedData = canBePublished ? PublishedData : null;

        // we *must* have either published or draft data
        // if it cannot be published, published data is going to be null
        // therefore, ensure that draft data is not
        if (draftData == null && !canBePublished)
        {
            draftData = PublishedData;
        }

        Node?.SetContentTypeAndData(contentType, draftData, publishedData, publishedSnapshotAccessor, variationContextAccessor, publishedModelFactory);
    }

    public ContentNodeKit Clone(IPublishedModelFactory publishedModelFactory)
        => new(new ContentNode(Node, publishedModelFactory), ContentTypeId, DraftData, PublishedData);

    public ContentNodeKit Clone(IPublishedModelFactory publishedModelFactory, ContentData draftData, ContentData publishedData)
        => new(new ContentNode(Node, publishedModelFactory), ContentTypeId, draftData, publishedData);
}
