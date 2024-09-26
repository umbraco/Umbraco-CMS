using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class PublishedContent : PublishedContentBase
{
    private IPublishedProperty[] _properties;
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
        IVariationContextAccessor variationContextAccessor)
        : base(variationContextAccessor)
    {
        VariationContextAccessor = variationContextAccessor;
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

    [Obsolete]
    public override string Path => GetPath();

    // This is ugly, but we need to still support Path because it's used to calculate permissions and stuff
    // We should really remove path in its entirety.
    private string GetPath()
    {
        IIdKeyMap idKeyMap = StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>();
        INavigationQueryService? navigationQueryService = null;
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

        Guid? parentKey;
        List<Guid> path = new();
        path.Add(Key);
        navigationQueryService.TryGetParentKey(Key, out parentKey);

        while (parentKey is not null)
        {
            path.Add(parentKey.Value);
            navigationQueryService.TryGetParentKey(parentKey.Value, out Guid? newParentKey);
            parentKey = newParentKey;
        }

        // Resolve path to IDs
        UmbracoObjectTypes objectType = _contentNode.ContentType.ItemType == PublishedItemType.Content ? UmbracoObjectTypes.Document : UmbracoObjectTypes.Media;
        List<int> idPath = new();

        foreach (Guid key in path)
        {
            Attempt<int> attempt = idKeyMap.GetIdForKey(key, objectType);
            if (attempt.Success is false)
            {
                throw new InvalidOperationException("Could not resolve path to IDs.");
            }

            idPath.Add(attempt.Result);
        }

        idPath.Reverse();
        // Path always starts with -1 as that's root
        return "-1," + string.Join(',', idPath);
    }

    public override int? TemplateId { get; }

    public override int CreatorId { get; }

    public override DateTime CreateDate { get; }

    public override int WriterId { get; }

    public override DateTime UpdateDate { get; }

    public bool IsPreviewing { get; } = false;

    // Needed for publishedProperty
    internal IVariationContextAccessor VariationContextAccessor { get; }

    [Obsolete("Use the INavigationQueryService instead, scheduled for removal in v17")]
    public override int Level
    {
        get
        {
            INavigationQueryService? navigationQueryService = null;
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

            navigationQueryService.TryGetLevel(Key, out int level);
            return level;
        }
    }

    [Obsolete("Please use IDocumentNavigationQueryService.TryGetParentKey() instead. Scheduled for removal in V16.")]
    public override IPublishedContent? Parent => this.Parent<IPublishedContent>(StaticServiceProvider.Instance.GetRequiredService<IPublishedContentCache>(), StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>());

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
