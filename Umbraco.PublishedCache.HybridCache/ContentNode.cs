using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

// represents a content "node" ie a pair of draft + published versions
// internal, never exposed, to be accessed from ContentStore (only!)
internal class ContentNode
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
    private IPublishedModelFactory? _publishedModelFactory;
    private IPublishedSnapshotAccessor? _publishedSnapshotAccessor;

    private IVariationContextAccessor? _variationContextAccessor;

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

    public IPublishedContent? DraftModel => GetModel(ref _draftModel, _draftData);

    public IPublishedContent? PublishedModel => GetModel(ref _publishedModel, _publishedData);

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
        ContentData? publishedData,
        IVariationContextAccessor variationContextAccessor,
        IPublishedModelFactory publishedModelFactory)
    {
        ContentType = contentType;

        if (draftData == null && publishedData == null)
        {
            throw new ArgumentException("Both draftData and publishedData cannot be null at the same time.");
        }

        _variationContextAccessor = variationContextAccessor;
        _publishedModelFactory = publishedModelFactory;

        _draftData = draftData;
        _publishedData = publishedData;
    }

    public bool HasPublishedCulture(string culture) => _publishedData != null && (_publishedData.CultureInfos?.ContainsKey(culture) ?? false);

    private IPublishedContent? GetModel(ref IPublishedContent? model, ContentData? contentData)
    {
        if (model != null)
        {
            return model;
        }

        if (contentData == null)
        {
            return null;
        }

        // create the model - we want to be fast, so no lock here: we may create
        // more than 1 instance, but the lock below ensures we only ever return
        // 1 unique instance - and locking is a nice explicit way to ensure this
        IPublishedContent? m = new PublishedContent(this, contentData, _variationContextAccessor!).CreateModel(_publishedModelFactory);

        // locking 'this' is not a best-practice but ContentNode is internal and
        // we know what we do, so it is fine here and avoids allocating an object
        lock (this)
        {
            return model ??= m;
        }
    }
#pragma warning restore IDE1006 // Naming Styles
}
