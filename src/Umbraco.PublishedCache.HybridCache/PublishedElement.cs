using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class PublishedElement : PublishableContentBase, IPublishedElement
{
    private IPublishedProperty[] _properties;
    private IReadOnlyDictionary<string, PublishedCultureInfo>? _cultures;
    private readonly string? _urlSegment;
    private readonly IReadOnlyDictionary<string, CultureVariation>? _cultureInfos;
    private readonly string _contentName;
    private readonly bool _published;

    public PublishedElement(
        ContentNode contentNode,
        bool preview,
        IElementsCache elementsCache,
        IVariationContextAccessor variationContextAccessor)
    {
        VariationContextAccessor = variationContextAccessor;
        ContentNode = contentNode;
        ContentData? contentData = preview ? ContentNode.DraftModel : ContentNode.PublishedModel;
        if (contentData is null)
        {
            throw new ArgumentNullException(nameof(contentData));
        }
        ContentData = contentData;

        _cultureInfos = contentData.CultureInfos;
        _contentName = contentData.Name;
        _urlSegment = contentData.UrlSegment;
        _published = contentData.Published;

        var properties = new IPublishedProperty[ContentNode.ContentType.PropertyTypes.Count()];
        var i = 0;
        foreach (IPublishedPropertyType propertyType in ContentNode.ContentType.PropertyTypes)
        {
            // add one property per property type - this is required, for the indexing to work
            // if contentData supplies pdatas, use them, else use null
            contentData.Properties.TryGetValue(propertyType.Alias, out PropertyData[]? propertyDatas); // else will be null
            properties[i++] = new PublishedProperty(propertyType, this, variationContextAccessor, preview, propertyDatas, elementsCache, propertyType.CacheLevel);
        }

        _properties = properties;

        Id = contentNode.Id;
        Key = contentNode.Key;
        CreatorId = contentNode.CreatorId;
        CreateDate = contentNode.CreateDate;
        SortOrder = contentNode.SortOrder;
        WriterId = contentData.WriterId;
        UpdateDate = contentData.VersionDate;
    }

    protected ContentNode ContentNode { get; }

    protected ContentData ContentData { get; }

    public override IPublishedContentType ContentType => ContentNode.ContentType;

    public override Guid Key { get; }

    public override IEnumerable<IPublishedProperty> Properties => _properties;

    public override int Id { get; }

    /// <inheritdoc />
    public string Name => this.Name(VariationContextAccessor);

    public override int SortOrder { get; }

    public override int CreatorId { get; }

    public override DateTime CreateDate { get; }

    public override int WriterId { get; }

    public override DateTime UpdateDate { get; }

    // Needed for publishedProperty
    internal IVariationContextAccessor VariationContextAccessor { get; }

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
                    { string.Empty, new PublishedCultureInfo(string.Empty, _contentName, _urlSegment, CreateDate) },
                };
            }

            if (_cultureInfos == null)
            {
                throw new PanicException("_contentDate.CultureInfos is null.");
            }


            return _cultures = _cultureInfos
                .ToDictionary(
                    x => x.Key,
                    x => new PublishedCultureInfo(x.Key, x.Value.Name, x.Value.UrlSegment, x.Value.Date),
                    StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <inheritdoc/>
    public override PublishedItemType ItemType => ContentNode.ContentType.ItemType;

    public override IPublishedProperty? GetProperty(string alias)
    {
        var index = ContentNode.ContentType.GetPropertyIndex(alias);
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
        if (_published)
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
        return _cultureInfos is not null && _cultureInfos.TryGetValue(culture, out CultureVariation? cvar) && cvar.IsDraft;
    }

    public override bool IsPublished(string? culture = null)
    {
        // whether we are the 'draft' or 'published' content, need to determine whether
        // there is a 'published' version for the specified culture (or at all, for
        // invariant content items)

        // if there is no 'published' published content, no culture can be published
        if (!ContentNode.HasPublished)
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
        return ContentNode.HasPublishedCulture(culture);
    }
}
