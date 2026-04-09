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

/// <summary>
/// Represents a published element that provides access to content type information and properties.
/// </summary>
/// <remarks>
/// A published element does NOT manage any tree-like elements. It represents detached content
/// that is not part of the content tree hierarchy (e.g., nested content items, block list items).
/// </remarks>
public class PublishedElement : IPublishedElement
{
    private readonly IPublishedProperty[] _propertiesArray;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedElement"/> class
    /// within the context of a published snapshot service (e.g., a published content property value).
    /// </summary>
    /// <param name="contentType">The published content type.</param>
    /// <param name="key">The unique identifier for this element.</param>
    /// <param name="values">The property values dictionary.</param>
    /// <param name="previewing">A value indicating whether this is a preview request.</param>
    /// <param name="referenceCacheLevel">The reference cache level for property value caching.</param>
    /// <param name="variationContext">The variation context for culture and segment.</param>
    /// <param name="cacheManager">The cache manager for property value caching.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is an empty GUID.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> or <paramref name="contentType"/> is null.</exception>
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
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedElement"/> class
    /// without any context, making it purely standalone and not interfering with the published snapshot service.
    /// </summary>
    /// <param name="contentType">The published content type.</param>
    /// <param name="key">The unique identifier for this element.</param>
    /// <param name="values">The property values dictionary.</param>
    /// <param name="previewing">A value indicating whether this is a preview request.</param>
    /// <param name="variationContext">The variation context for culture and segment.</param>
    /// <remarks>
    /// Using an initial reference cache level of <see cref="PropertyCacheLevel.None"/> ensures that everything will be
    /// cached at element level, and that reference cache level will propagate to all properties.
    /// </remarks>
    public PublishedElement(IPublishedContentType contentType, Guid key, Dictionary<string, object?> values, bool previewing, VariationContext variationContext)
        : this(contentType, key, values, previewing, PropertyCacheLevel.None, variationContext, null)
    {
    }

    /// <inheritdoc />
    public IPublishedContentType ContentType { get; }

    /// <inheritdoc />
    public Guid Key { get; }

    /// <summary>
    /// Converts a property values dictionary to use case-insensitive key comparison.
    /// </summary>
    /// <param name="values">The original property values dictionary.</param>
    /// <returns>A dictionary with case-insensitive key comparison.</returns>
    private static Dictionary<string, object?> GetCaseInsensitiveValueDictionary(Dictionary<string, object?> values)
    {
        // ensure we ignore case for property aliases
        IEqualityComparer<string> comparer = values.Comparer;
        var ignoreCase = Equals(comparer, StringComparer.OrdinalIgnoreCase) ||
                         Equals(comparer, StringComparer.InvariantCultureIgnoreCase) ||
                         Equals(comparer, StringComparer.CurrentCultureIgnoreCase);
        return ignoreCase ? values : new Dictionary<string, object?>(values, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public IEnumerable<IPublishedProperty> Properties => _propertiesArray;

    /// <inheritdoc />
    public IPublishedProperty? GetProperty(string alias)
    {
        var index = ContentType.GetPropertyIndex(alias);
        IPublishedProperty? property = index < 0 ? null : _propertiesArray?[index];
        return property;
    }
}
