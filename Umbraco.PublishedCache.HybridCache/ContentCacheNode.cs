namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class ContentCacheNode
{
    public ContentCacheNode()
    {

    }

    public ContentCacheNode(ContentData? draft, ContentData? published)
    {
        Draft = draft;
        Published = published;
    }

    public int Id { get; set; }

    public Guid Key { get; set; }

    public string Path { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreateDate { get; set; }

    public int CreatorId { get; set; }

    public int ContentTypeId { get; set; }

    public ContentData? Draft { get; set; }

    public ContentData? Published { get; set; }
}
