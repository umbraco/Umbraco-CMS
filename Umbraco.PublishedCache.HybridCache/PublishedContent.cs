using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class PublishedContent : PublishedContentBase
{
    // TODO: We should refactor this, it doesn't make a lot of sense to have both content node and content data
    // since content node contains the content data for both draft and published
    private IPublishedProperty[] _properties;
    private readonly ContentData _contentData;
    private readonly ContentNode _contentNode;
    private IReadOnlyDictionary<string, PublishedCultureInfo>? _cultures;
    private readonly string? _urlSegment;

    public PublishedContent(
        ContentNode contentNode,
        ContentData contentData,
        IElementsCache elementsCache,
        IVariationContextAccessor variationContextAccessor)
        : base(variationContextAccessor)
    {
        VariationContextAccessor = variationContextAccessor;
        _contentNode = contentNode;

        _contentData = contentData ?? throw new ArgumentNullException(nameof(contentData));
        _urlSegment = _contentData.UrlSegment;

        var properties = new IPublishedProperty[_contentNode.ContentType.PropertyTypes.Count()];
        var i = 0;
        foreach (IPublishedPropertyType propertyType in _contentNode.ContentType.PropertyTypes)
        {
            // add one property per property type - this is required, for the indexing to work
            // if contentData supplies pdatas, use them, else use null
            contentData.Properties.TryGetValue(propertyType.Alias, out PropertyData[]? propertyDatas); // else will be null
            properties[i++] = new PublishedProperty(propertyType, this, propertyDatas, elementsCache, propertyType.CacheLevel);
        }

        _properties = properties;

        Id = contentNode.Id;
        Key = contentNode.Key;
        CreatorId = contentNode.CreatorId;
        CreateDate = contentNode.CreateDate;
        SortOrder = contentNode.SortOrder;
        WriterId = contentData.WriterId;
        TemplateId = contentData.TemplateId;
        UpdateDate = contentData.VersionDate;
    }

    public override IPublishedContentType ContentType => _contentNode.ContentType;

    public override Guid Key { get; }

    public override IEnumerable<IPublishedProperty> Properties => _properties;

    public override int Id { get; }

    public override int SortOrder { get; }

    // TODO: Remove path.
    public override string Path => string.Empty;

    public override int? TemplateId { get; }

    public override int CreatorId { get; }

    public override DateTime CreateDate { get; }

    public override int WriterId { get; }

    public override DateTime UpdateDate { get; }

    public bool IsPreviewing { get; } = false;

    // Needed for publishedProperty
    internal IVariationContextAccessor VariationContextAccessor { get; }

    public override int Level { get; } = 0;

    public override IEnumerable<IPublishedContent> ChildrenForAllCultures { get; } = Enumerable.Empty<IPublishedContent>();

    public override IPublishedContent? Parent { get; } = null!;


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
                    { string.Empty, new PublishedCultureInfo(string.Empty, _contentData.Name, _urlSegment, CreateDate) },
                };
            }

            if (_contentData.CultureInfos == null)
            {
                throw new PanicException("_contentDate.CultureInfos is null.");
            }

            return _cultures = _contentData.CultureInfos
                .ToDictionary(
                    x => x.Key,
                    x => new PublishedCultureInfo(x.Key, x.Value.Name, x.Value.UrlSegment, x.Value.Date),
                    StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <inheritdoc/>
    public override PublishedItemType ItemType => _contentNode.ContentType.ItemType;

    public override IPublishedProperty? GetProperty(string alias)
    {
        var index = _contentNode.ContentType.GetPropertyIndex(alias);
        if (index < 0)
        {
            return null; // happens when 'alias' does not match a content type property alias
        }

        // should never happen - properties array must be in sync with property type
        if (index >= _properties.Length)
        {
            throw new IndexOutOfRangeException(
                "Index points outside the properties array, which means the properties array is corrupt.");
        }

        IPublishedProperty property = _properties[index];
        return property;
    }

    public override bool IsDraft(string? culture = null)
    {
        // if this is the 'published' published content, nothing can be draft
        if (_contentData.Published)
        {
            return false;
        }

        // not the 'published' published content, and does not vary = must be draft
        if (!ContentType.VariesByCulture())
        {
            return true;
        }

        // handle context culture
        culture ??= VariationContextAccessor?.VariationContext?.Culture ?? string.Empty;

        // not the 'published' published content, and varies
        // = depends on the culture
        return _contentData.CultureInfos is not null && _contentData.CultureInfos.TryGetValue(culture, out CultureVariation? cvar) && cvar.IsDraft;
    }

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
        culture ??= VariationContextAccessor.VariationContext?.Culture ?? string.Empty;

        // there is a 'published' published content, and varies
        // = depends on the culture
        return _contentNode.HasPublishedCulture(culture);
    }
}
