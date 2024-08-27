using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache;

// represents a content "node" ie a pair of draft + published versions
// internal, never exposed, to be accessed from ContentStore (only!)
internal sealed class ContentNode
{
    // everything that is common to both draft and published versions
    // keep this as small as possible
#pragma warning disable IDE1006 // Naming Styles
    public readonly int Id;

    private ContentData? _draftData;

    // draft and published version (either can be null, but not both)
    // are models not direct PublishedContent instances
    private IPublishedContent? _draftModel;
    private ContentData? _publishedData;
    private IPublishedContent? _publishedModel;
    private IPublishedSnapshotAccessor? _publishedSnapshotAccessor;

    // special ctor for root pseudo node
    public ContentNode() => Path = string.Empty;

    // special ctor with no content data - for members
    public ContentNode(
        int id,
        Guid uid,
        IPublishedContentType contentType,
        string path,
        int sortOrder,
        DateTime createDate,
        int creatorId)
        : this()
    {
        Id = id;
        Uid = uid;
        ContentType = contentType;
        Path = path;
        SortOrder = sortOrder;
        CreateDate = createDate;
        CreatorId = creatorId;
    }

    // 2-phases ctor, phase 1
    public ContentNode(
        int id,
        Guid uid,
        string path,
        int sortOrder,
        DateTime createDate,
        int creatorId)
    {
        Id = id;
        Uid = uid;
        Path = path;
        SortOrder = sortOrder;
        CreateDate = createDate;
        CreatorId = creatorId;
    }

    public bool HasPublished => _publishedData != null;

    public ContentData? DraftModel => _draftData;

    public ContentData? PublishedModel => _publishedData;

    public readonly Guid Uid;
    public IPublishedContentType ContentType = null!;
    public readonly string Path;
    public readonly int SortOrder;
    public readonly DateTime CreateDate;
    public readonly int CreatorId;

    // two-phase ctor, phase 2
    public void SetContentTypeAndData(
        IPublishedContentType contentType,
        ContentData? draftData,
        ContentData? publishedData)
    {
        ContentType = contentType;

        if (draftData == null && publishedData == null)
        {
            throw new ArgumentException("Both draftData and publishedData cannot be null at the same time.");
        }

        _draftData = draftData;
        _publishedData = publishedData;
    }

    public bool HasPublishedCulture(string culture) => _publishedData != null && (_publishedData.CultureInfos?.ContainsKey(culture) ?? false);
#pragma warning restore IDE1006 // Naming Styles
}
