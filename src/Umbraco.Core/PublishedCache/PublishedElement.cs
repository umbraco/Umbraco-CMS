using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.PublishedCache;

// notes:
// a published element does NOT manage any tree-like elements, neither the
// original NestedContent (from Lee) nor the DetachedPublishedContent POC did.
//
// at the moment we do NOT support models for sets - that would require
// an entirely new models factory + not even sure it makes sense at all since
// sets are created manually todo yes it does! - what does this all mean?
//
public class PublishedElement : IPublishedElement
{
    private readonly IPublishedProperty[] _propertiesArray;

    // initializes a new instance of the PublishedElement class
    // within the context of a published snapshot service (eg a published content property value)
    public PublishedElement(IPublishedContentType contentType, Guid key, Dictionary<string, object?>? values, bool previewing, PropertyCacheLevel referenceCacheLevel, VariationContext variationContext, ICacheManager? cacheManager)
    {
        if (key == Guid.Empty)
        {
            throw new ArgumentException("Empty guid.");
        }

        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        Key = key;

        values = GetCaseInsensitiveValueDictionary(values);

        _propertiesArray = contentType
                               .PropertyTypes?
                               .Select(propertyType =>
                               {
                                   values.TryGetValue(propertyType.Alias, out var value);
                                   return (IPublishedProperty)new PublishedElementPropertyBase(propertyType, this, previewing, referenceCacheLevel, variationContext, cacheManager, value);
                               })
                               .ToArray()
                           ?? [];

        Name = string.Empty;
        Path = string.Empty;
        Cultures = new Dictionary<string, PublishedCultureInfo>();
    }

    // initializes a new instance of the PublishedElement class
    // without any context, so it's purely 'standalone' and should NOT interfere with the published snapshot service
    // + using an initial reference cache level of .None ensures that everything will be
    // cached at .Content level - and that reference cache level will propagate to all
    // properties
    public PublishedElement(IPublishedContentType contentType, Guid key, Dictionary<string, object?> values, bool previewing, VariationContext variationContext)
        : this(contentType, key, values, previewing, PropertyCacheLevel.None, variationContext, null)
    {
    }

    public IPublishedContentType ContentType { get; }

    public Guid Key { get; }

    private static Dictionary<string, object?> GetCaseInsensitiveValueDictionary(Dictionary<string, object?> values)
    {
        // ensure we ignore case for property aliases
        IEqualityComparer<string> comparer = values.Comparer;
        var ignoreCase = Equals(comparer, StringComparer.OrdinalIgnoreCase) ||
                         Equals(comparer, StringComparer.InvariantCultureIgnoreCase) ||
                         Equals(comparer, StringComparer.CurrentCultureIgnoreCase);
        return ignoreCase ? values : new Dictionary<string, object?>(values, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<IPublishedProperty> Properties => _propertiesArray;

    public IPublishedProperty? GetProperty(string alias)
    {
        var index = ContentType.GetPropertyIndex(alias);
        IPublishedProperty? property = index < 0 ? null : _propertiesArray?[index];
        return property;
    }

    // TODO ELEMENTS: figure out what to do with all these
    //                perhaps replace this whole class with PublishedElementWrapped? in that case, we also need to do the same for PublishedContent
    public int Id { get; }
    public string Name { get; }
    public int SortOrder { get; }
    public int Level { get; }
    public string Path { get; }
    public int CreatorId { get; }
    public DateTime CreateDate { get; }
    public int WriterId { get; }
    public DateTime UpdateDate { get; }
    public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; }
    public PublishedItemType ItemType { get; }
    public bool IsDraft(string? culture = null) => throw new NotImplementedException();
    public bool IsPublished(string? culture = null) => throw new NotImplementedException();
}
