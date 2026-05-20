using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class PublishedElement : PublishableContentBase, IPublishedElement
{
    /// <summary>
    /// Backing array of materialized properties for this element. Built lazily on first
    /// access via <see cref="EnsureProperties"/>; <c>null</c> until then.
    /// </summary>
    /// <remarks>
    /// Lazy construction avoids allocating a <see cref="PublishedProperty"/> wrapper per
    /// property type for traversal-only operations (e.g. <c>Children().Count()</c>,
    /// <c>Descendants()</c> without property reads), which is a significant slice of
    /// allocation for tree traversals.
    /// </remarks>
    private IPublishedProperty[]? _properties;

    private readonly IElementsCache _elementsCache;
    private IReadOnlyDictionary<string, PublishedCultureInfo>? _cultures;
    private readonly string? _urlSegment;
    private readonly IReadOnlyDictionary<string, CultureVariation>? _cultureInfos;
    private readonly string _contentName;
    private readonly bool _published;
    private readonly bool _isPreviewing;

    public PublishedElement(
        ContentNode contentNode,
        bool preview,
        IElementsCache elementsCache,
        IVariationContextAccessor variationContextAccessor,
        IPropertyRenderingContextAccessor propertyRenderingContextAccessor)
    {
        VariationContextAccessor = variationContextAccessor;
        PropertyRenderingContextAccessor = propertyRenderingContextAccessor;
        ContentNode = contentNode;
        ContentData? contentData = preview ? ContentNode.DraftModel : ContentNode.PublishedModel;
        if (contentData is null)
        {
            throw new ArgumentNullException(nameof(contentData));
        }
        ContentData = contentData;

        _elementsCache = elementsCache;
        _cultureInfos = contentData.CultureInfos;
        _contentName = contentData.Name;
        _urlSegment = contentData.UrlSegment;
        _published = contentData.Published;

        _isPreviewing = preview;

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

    public override IEnumerable<IPublishedProperty> Properties => EnsureProperties();

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

    internal IPropertyRenderingContextAccessor PropertyRenderingContextAccessor { get; }

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

        IPublishedProperty[] properties = EnsureProperties();

        // should never happen - properties array must be in sync with property type
        if (index >= properties.Length)
        {
            throw new IndexOutOfRangeException(
                "Index points outside the properties array, which means the properties array is corrupt.");
        }

        return properties[index];
    }

    private IPublishedProperty[] EnsureProperties()
    {
        IPublishedProperty[]? properties = _properties;
        if (properties is not null)
        {
            return properties;
        }

        return BuildProperties();
    }

    private IPublishedProperty[] BuildProperties()
    {
        IEnumerable<IPublishedPropertyType> propertyTypes = ContentNode.ContentType.PropertyTypes;
        var newProperties = new IPublishedProperty[propertyTypes.Count()];
        var i = 0;
        foreach (IPublishedPropertyType propertyType in propertyTypes)
        {
            // add one property per property type - this is required for the indexing to work
            // if ContentData supplies pdatas, use them, else use null
            ContentData.Properties.TryGetValue(propertyType.Alias, out PropertyData[]? propertyDatas);
            newProperties[i++] = new PublishedProperty(propertyType, this, VariationContextAccessor, PropertyRenderingContextAccessor, _isPreviewing, propertyDatas, _elementsCache, propertyType.CacheLevel);
        }

        // Use CompareExchange so concurrent first-access threads agree on a single canonical
        // array — losers discard their newly built array and use the winner's.
        return Interlocked.CompareExchange(ref _properties, newProperties, null) ?? newProperties;
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
            // In preview mode, the ContentNode only has draft data loaded (published data
            // is stored in a separate cache entry). Fall back to IDocumentPublishStatusQueryService
            // which is an in-memory service that tracks actual document publish status.
            if (_isPreviewing && ItemType == PublishedItemType.Content)
            {
                culture ??= VariationContextAccessor.VariationContext?.Culture ?? string.Empty;
                IDocumentPublishStatusQueryService publishStatusQueryService =
                    StaticServiceProvider.Instance.GetRequiredService<IDocumentPublishStatusQueryService>();
                return publishStatusQueryService.IsPublished(Key, culture);
            }

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
