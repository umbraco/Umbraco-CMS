using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class PublishedContent : PublishedContentBase
{
    /// <summary>
    /// Backing array of materialized properties for this content item. Built lazily on first
    /// access via <see cref="EnsureProperties"/>; <c>null</c> until then.
    /// </summary>
    /// <remarks>
    /// Lazy construction avoids allocating a <see cref="PublishedProperty"/> wrapper per
    /// property type for traversal-only operations (e.g. <c>Children().Count()</c>,
    /// <c>Descendants()</c> without property reads), which is a significant slice of
    /// allocation for tree traversals.
    /// </remarks>
    private IPublishedProperty[]? _properties;

    private readonly Dictionary<string, PropertyData[]> _propertyData;
    private readonly IElementsCache _elementsCache;

    private readonly ContentNode _contentNode;
    private IReadOnlyDictionary<string, PublishedCultureInfo>? _cultures;
    private readonly string? _urlSegment;
    private readonly IReadOnlyDictionary<string, CultureVariation>? _cultureInfos;
    private readonly string _contentName;
    private readonly bool _published;

    public PublishedContent(
        ContentNode contentNode,
        bool preview,
        IElementsCache elementsCache,
        IVariationContextAccessor variationContextAccessor,
        IPropertyRenderingContextAccessor propertyRenderingContextAccessor)
        : base(variationContextAccessor)
    {
        VariationContextAccessor = variationContextAccessor;
        PropertyRenderingContextAccessor = propertyRenderingContextAccessor;
        _contentNode = contentNode;
        ContentData? contentData = preview ? _contentNode.DraftModel : _contentNode.PublishedModel;
        if (contentData is null)
        {
            throw new ArgumentNullException(nameof(contentData));
        }

        _cultureInfos = contentData.CultureInfos;
        _contentName = contentData.Name;
        _urlSegment = contentData.UrlSegment;
        _published = contentData.Published;
        _propertyData = contentData.Properties;
        _elementsCache = elementsCache;

        IsPreviewing = preview;

        Id = contentNode.Id;
        Key = contentNode.Key;
        CreatorId = contentNode.CreatorId;
        CreateDate = contentNode.CreateDate.EnsureUtc();
        SortOrder = contentNode.SortOrder;
        WriterId = contentData.WriterId;
        TemplateId = contentData.TemplateId;
        UpdateDate = contentData.VersionDate.EnsureUtc();
    }

    public override IPublishedContentType ContentType => _contentNode.ContentType;

    public override Guid Key { get; }

    public override IEnumerable<IPublishedProperty> Properties => EnsureProperties();

    public override int Id { get; }

    public override int SortOrder { get; }

    [Obsolete]
    public override string Path
    {
        get
        {
            IDocumentNavigationQueryService documentNavigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();
            IIdKeyMap idKeyMap = StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>();


            if (documentNavigationQueryService.TryGetAncestorsOrSelfKeys(Key, out var ancestorsOrSelfKeys))
            {
                var sb = new StringBuilder("-1");
                foreach (Guid ancestorsOrSelfKey in ancestorsOrSelfKeys.Reverse())
                {
                    Attempt<int> idAttempt = idKeyMap.GetIdForKey(ancestorsOrSelfKey, GetObjectType());
                    if (idAttempt.Success)
                    {
                        sb.AppendFormat(",{0}", idAttempt.Result);
                    }
                }

                return sb.ToString();
            }

            return string.Empty;
        }
    }

    private UmbracoObjectTypes GetObjectType()
    {
        switch (ItemType)
        {
            case PublishedItemType.Content:
                return UmbracoObjectTypes.Document;
            case PublishedItemType.Media:
                return UmbracoObjectTypes.Media;
            case PublishedItemType.Member:
                return UmbracoObjectTypes.Member;
            default:
                return UmbracoObjectTypes.Unknown;
        }
    }

    public override int? TemplateId { get; }

    public override int CreatorId { get; }

    public override DateTime CreateDate { get; }

    public override int WriterId { get; }

    public override DateTime UpdateDate { get; }

    public bool IsPreviewing { get; }

    // Needed for publishedProperty
    internal IVariationContextAccessor VariationContextAccessor { get; }

    internal IPropertyRenderingContextAccessor PropertyRenderingContextAccessor { get; }

    [Obsolete("Use the INavigationQueryService instead. Scheduled for removal in Umbraco 18.")]
    public override int Level
    {
        get
        {
            INavigationQueryService? navigationQueryService;
            switch (_contentNode.ContentType.ItemType)
            {
                case PublishedItemType.Content:
                    navigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();
                    break;
                case PublishedItemType.Media:
                    navigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>();
                    break;
                default:
                    throw new NotImplementedException("Level is not implemented for " + _contentNode.ContentType.ItemType);
            }

            // Attempt to retrieve the level, returning 0 if it fails or if level is null.
            if (navigationQueryService.TryGetLevel(Key, out var level) && level.HasValue)
            {
                return level.Value;
            }

            return 0;
        }
    }

    [Obsolete("Please use TryGetParentKey() on IDocumentNavigationQueryService or IMediaNavigationQueryService instead. Scheduled for removal in Umbraco 18.")]
    public override IPublishedContent? Parent => GetParent();

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
    public override PublishedItemType ItemType => _contentNode.ContentType.ItemType;

    public override IPublishedProperty? GetProperty(string alias)
    {
        var index = _contentNode.ContentType.GetPropertyIndex(alias);
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
        IEnumerable<IPublishedPropertyType> propertyTypes = _contentNode.ContentType.PropertyTypes;
        var newProperties = new IPublishedProperty[propertyTypes.Count()];
        var i = 0;
        foreach (IPublishedPropertyType propertyType in propertyTypes)
        {
            // add one property per property type - this is required for the indexing to work
            // if propertyData supplies pdatas, use them, else use null
            _propertyData.TryGetValue(propertyType.Alias, out PropertyData[]? propertyDatas);
            newProperties[i++] = new PublishedProperty(propertyType, this, propertyDatas, _elementsCache, propertyType.CacheLevel);
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
        if (!_contentNode.HasPublished)
        {
            // In preview mode, the ContentNode only has draft data loaded (published data
            // is stored in a separate cache entry). Fall back to IPublishStatusQueryService
            // which is an in-memory service that tracks actual document publish status.
            if (IsPreviewing && ItemType == PublishedItemType.Content)
            {
                culture ??= VariationContextAccessor.VariationContext?.Culture ?? string.Empty;
                IPublishStatusQueryService publishStatusQueryService =
                    StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>();
                return publishStatusQueryService.IsDocumentPublished(Key, culture);
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
        return _contentNode.HasPublishedCulture(culture);
    }

    private IPublishedContent? GetParent()
    {
        INavigationQueryService? navigationQueryService;
        IPublishedStatusFilteringService? publishedStatusFilteringService;

        switch (ContentType.ItemType)
        {
            case PublishedItemType.Content:
                navigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>();
                publishedStatusFilteringService = StaticServiceProvider.Instance.GetRequiredService<IPublishedContentStatusFilteringService>();
                break;
            case PublishedItemType.Media:
                navigationQueryService = StaticServiceProvider.Instance.GetRequiredService<IMediaNavigationQueryService>();
                publishedStatusFilteringService = StaticServiceProvider.Instance.GetRequiredService<IPublishedMediaStatusFilteringService>();
                break;
            default:
                throw new NotImplementedException("Level is not implemented for " + ContentType.ItemType);
        }

        return this.Parent<IPublishedContent>(navigationQueryService, publishedStatusFilteringService);
    }
}
