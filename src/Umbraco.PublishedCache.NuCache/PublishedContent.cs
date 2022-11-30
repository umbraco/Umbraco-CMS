using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

internal class PublishedContent : PublishedContentBase
{
    private readonly ContentNode _contentNode;
    private readonly IPublishedModelFactory? _publishedModelFactory;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly string? _urlSegment;

    #region Content Type

    /// <inheritdoc />
    public override IPublishedContentType ContentType => _contentNode.ContentType;

    #endregion

    #region PublishedElement

    /// <inheritdoc />
    public override Guid Key => _contentNode.Uid;

    #endregion

    #region Constructors

    public PublishedContent(
        ContentNode contentNode,
        ContentData contentData,
        IPublishedSnapshotAccessor? publishedSnapshotAccessor,
        IVariationContextAccessor? variationContextAccessor,
        IPublishedModelFactory? publishedModelFactory)
        : base(variationContextAccessor)
    {
        _contentNode = contentNode ?? throw new ArgumentNullException(nameof(contentNode));
        ContentData = contentData ?? throw new ArgumentNullException(nameof(contentData));
        _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                     throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        _publishedModelFactory = publishedModelFactory;
        VariationContextAccessor = variationContextAccessor ??
                                   throw new ArgumentNullException(nameof(variationContextAccessor));

        _urlSegment = ContentData.UrlSegment;
        IsPreviewing = ContentData.Published == false;

        var properties = new IPublishedProperty[_contentNode.ContentType.PropertyTypes.Count()];
        var i = 0;
        foreach (IPublishedPropertyType propertyType in _contentNode.ContentType.PropertyTypes)
        {
            // add one property per property type - this is required, for the indexing to work
            // if contentData supplies pdatas, use them, else use null
            contentData.Properties.TryGetValue(propertyType.Alias, out PropertyData[]? pdatas); // else will be null
            properties[i++] = new Property(propertyType, this, pdatas, _publishedSnapshotAccessor);
        }

        PropertiesArray = properties;
    }

    // used when cloning in ContentNode
    public PublishedContent(
        ContentNode contentNode,
        PublishedContent origin,
        IVariationContextAccessor variationContextAccessor)
        : base(variationContextAccessor)
    {
        _contentNode = contentNode;
        _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
        VariationContextAccessor = origin.VariationContextAccessor;
        ContentData = origin.ContentData;

        _urlSegment = origin._urlSegment;
        IsPreviewing = origin.IsPreviewing;

        // here is the main benefit: we do not re-create properties so if anything
        // is cached locally, we share the cache - which is fine - if anything depends
        // on the tree structure, it should not be cached locally to begin with
        PropertiesArray = origin.PropertiesArray;
    }

    // clone for previewing as draft a published content that is published and has no draft
    private PublishedContent(PublishedContent origin)
        : base(origin.VariationContextAccessor)
    {
        _publishedSnapshotAccessor = origin._publishedSnapshotAccessor;
        VariationContextAccessor = origin.VariationContextAccessor;
        _contentNode = origin._contentNode;
        ContentData = origin.ContentData;

        _urlSegment = origin._urlSegment;
        IsPreviewing = true;

        // clone properties so _isPreviewing is true
        PropertiesArray = origin.PropertiesArray.Select(x => (IPublishedProperty)new Property((Property)x, this))
            .ToArray();
    }

    #endregion

    #region Get Content/Media for Parent/Children

    // this is for tests purposes
    // args are: current published snapshot (may be null), previewing, content id - returns: content
    internal static Func<IPublishedSnapshot, bool, int, IPublishedContent?> GetContentByIdFunc { get; set; }
        = (publishedShapshot, previewing, id) => publishedShapshot.Content?.GetById(previewing, id);

    internal static Func<IPublishedSnapshot, bool, int, IPublishedContent?> GetMediaByIdFunc { get; set; }
        = (publishedShapshot, previewing, id) => publishedShapshot.Media?.GetById(previewing, id);

    private Func<IPublishedSnapshot, bool, int, IPublishedContent?> GetGetterById()
    {
        switch (ContentType.ItemType)
        {
            case PublishedItemType.Content:
                return GetContentByIdFunc;
            case PublishedItemType.Media:
                return GetMediaByIdFunc;
            default:
                throw new PanicException("invalid item type");
        }
    }

    #endregion

    #region PublishedContent

    internal ContentData ContentData { get; }

    /// <inheritdoc />
    public override int Id => _contentNode.Id;

    /// <inheritdoc />
    public override int SortOrder => _contentNode.SortOrder;

    /// <inheritdoc />
    public override int Level => _contentNode.Level;

    /// <inheritdoc />
    public override string Path => _contentNode.Path;

    /// <inheritdoc />
    public override int? TemplateId => ContentData.TemplateId;

    /// <inheritdoc />
    public override int CreatorId => _contentNode.CreatorId;

    /// <inheritdoc />
    public override DateTime CreateDate => _contentNode.CreateDate;

    /// <inheritdoc />
    public override int WriterId => ContentData.WriterId;

    /// <inheritdoc />
    public override DateTime UpdateDate => ContentData.VersionDate;

    // ReSharper disable once CollectionNeverUpdated.Local
    private static readonly IReadOnlyDictionary<string, PublishedCultureInfo> EmptyCultures =
        new Dictionary<string, PublishedCultureInfo>();

    private IReadOnlyDictionary<string, PublishedCultureInfo>? _cultures;

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, PublishedCultureInfo> Cultures
    {
        get
        {
            if (_cultures != null)
            {
                return _cultures;
            }

            if (!ContentType.VariesByCulture())
            {
                return _cultures = new Dictionary<string, PublishedCultureInfo>
                {
                    { string.Empty, new PublishedCultureInfo(string.Empty, ContentData.Name, _urlSegment, CreateDate) },
                };
            }

            if (ContentData.CultureInfos == null)
            {
                throw new PanicException("_contentDate.CultureInfos is null.");
            }

            return _cultures = ContentData.CultureInfos
                .ToDictionary(
                    x => x.Key,
                    x => new PublishedCultureInfo(x.Key, x.Value.Name, x.Value.UrlSegment, x.Value.Date),
                    StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <inheritdoc />
    public override PublishedItemType ItemType => _contentNode.ContentType.ItemType;

    /// <inheritdoc />
    public override bool IsDraft(string? culture = null)
    {
        // if this is the 'published' published content, nothing can be draft
        if (ContentData.Published)
        {
            return false;
        }

        // not the 'published' published content, and does not vary = must be draft
        if (!ContentType.VariesByCulture())
        {
            return true;
        }

        // handle context culture
        if (culture == null)
        {
            culture = VariationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        // not the 'published' published content, and varies
        // = depends on the culture
        return ContentData.CultureInfos is not null &&
               ContentData.CultureInfos.TryGetValue(culture, out CultureVariation? cvar) && cvar.IsDraft;
    }

    /// <inheritdoc />
    public override bool IsPublished(string? culture = null)
    {
        // whether we are the 'draft' or 'published' content, need to determine whether
        // there is a 'published' version for the specified culture (or at all, for
        // invariant content items)

        // if there is no 'published' published content, no culture can be published
        if (!_contentNode.HasPublished)
        {
            return false;
        }

        // if there is a 'published' published content, and does not vary = published
        if (!ContentType.VariesByCulture())
        {
            return true;
        }

        // handle context culture
        if (culture == null)
        {
            culture = VariationContextAccessor?.VariationContext?.Culture ?? string.Empty;
        }

        // there is a 'published' published content, and varies
        // = depends on the culture
        return _contentNode.HasPublishedCulture(culture);
    }

    #endregion

    #region Tree

    /// <inheritdoc />
    public override IPublishedContent? Parent
    {
        get
        {
            Func<IPublishedSnapshot, bool, int, IPublishedContent?> getById = GetGetterById();
            IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            return getById(publishedSnapshot, IsPreviewing, ParentId);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IPublishedContent> ChildrenForAllCultures
    {
        get
        {
            Func<IPublishedSnapshot, bool, int, IPublishedContent?> getById = GetGetterById();
            IPublishedSnapshot publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
            var id = _contentNode.FirstChildContentId;

            while (id > 0)
            {
                // is IsPreviewing is false, then this can return null
                IPublishedContent? content = getById(publishedSnapshot, IsPreviewing, id);

                if (content != null)
                {
                    yield return content;
                }
                else
                {
                    // but if IsPreviewing is true, we should have a child
                    if (IsPreviewing)
                    {
                        throw new PanicException($"failed to get content with id={id}");
                    }

                    // if IsPreviewing is false, get the unpublished child nevertheless
                    // we need it to keep enumerating children! but we don't return it
                    content = getById(publishedSnapshot, true, id);
                    if (content == null)
                    {
                        throw new PanicException($"failed to get content with id={id}");
                    }
                }

                var next = UnwrapIPublishedContent(content)._contentNode.NextSiblingContentId;

#if DEBUG
                // I've seen this happen but I think that may have been due to corrupt DB data due to my own
                // bugs, but I'm leaving this here just in case we encounter it again while we're debugging.
                if (next == id)
                {
                    throw new PanicException($"The current content id {id} is the same as it's next sibling id {next}");
                }
#endif

                id = next;
            }
        }
    }

    #endregion

    #region Properties

    /// <inheritdoc cref="IPublishedElement.Properties" />
    public override IEnumerable<IPublishedProperty> Properties => PropertiesArray;

    /// <inheritdoc cref="IPublishedElement.GetProperty(string)" />
    public override IPublishedProperty? GetProperty(string alias)
    {
        var index = _contentNode.ContentType.GetPropertyIndex(alias);
        if (index < 0)
        {
            return null; // happens when 'alias' does not match a content type property alias
        }

        // should never happen - properties array must be in sync with property type
        if (index >= PropertiesArray.Length)
        {
            throw new IndexOutOfRangeException(
                "Index points outside the properties array, which means the properties array is corrupt.");
        }

        IPublishedProperty property = PropertiesArray[index];
        return property;
    }

    #endregion

    #region Caching

    // beware what you use that one for - you don't want to cache its result
    private IAppCache? GetAppropriateCache()
    {
        IPublishedSnapshot? publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
        IAppCache? cache = publishedSnapshot == null
            ? null
            : (IsPreviewing == false || PublishedSnapshotService.FullCacheWhenPreviewing) &&
              ContentType.ItemType != PublishedItemType.Member
                ? publishedSnapshot.ElementsCache
                : publishedSnapshot.SnapshotCache;
        return cache;
    }

    private IAppCache? GetCurrentSnapshotCache()
    {
        IPublishedSnapshot? publishedSnapshot = _publishedSnapshotAccessor.GetRequiredPublishedSnapshot();
        return publishedSnapshot?.SnapshotCache;
    }

    #endregion

    #region Internal

    // used by property
    internal IVariationContextAccessor VariationContextAccessor { get; }

    // used by navigable content
    internal IPublishedProperty[] PropertiesArray { get; }

    // used by navigable content
    internal int ParentId => _contentNode.ParentContentId;

    // used by navigable content
    // includes all children, published or unpublished
    // NavigableNavigator takes care of selecting those it wants
    // note: this is not efficient - we do not try to be (would require a double-linked list)
    internal IList<int>? ChildIds => Children?.Select(x => x.Id).ToList();

    // used by Property
    // gets a value indicating whether the content or media exists in
    // a previewing context or not, ie whether its Parent, Children, and
    // properties should refer to published, or draft content
    internal bool IsPreviewing { get; }

    private string? _asPreviewingCacheKey;

    private string AsPreviewingCacheKey =>
        _asPreviewingCacheKey ?? (_asPreviewingCacheKey = CacheKeys.PublishedContentAsPreviewing(Key));

    // used by ContentCache
    internal IPublishedContent? AsDraft()
    {
        if (IsPreviewing)
        {
            return this;
        }

        IAppCache? cache = GetAppropriateCache();
        if (cache == null)
        {
            return new PublishedContent(this).CreateModel(_publishedModelFactory);
        }

        return (IPublishedContent?)cache.Get(AsPreviewingCacheKey, () => new PublishedContent(this).CreateModel(_publishedModelFactory));
    }

    // used by Navigable.Source,...
    internal static PublishedContent UnwrapIPublishedContent(IPublishedContent content)
    {
        while (content is PublishedContentWrapped wrapped)
        {
            content = wrapped.Unwrap();
        }

        if (!(content is PublishedContent inner))
        {
            throw new InvalidOperationException("Innermost content is not PublishedContent.");
        }

        return inner;
    }

    #endregion
}
