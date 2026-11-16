using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for element picker properties.
/// </summary>
public class ElementPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IPublishedElementCache _publishedElementCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IApiElementBuilder _apiElementBuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ElementPickerValueConverter" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="publishedElementCache">The published element cache.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="apiElementBuilder">The API element builder.</param>
    public ElementPickerValueConverter(
        IJsonSerializer jsonSerializer,
        IPublishedElementCache publishedElementCache,
        IVariationContextAccessor variationContextAccessor,
        IApiElementBuilder apiElementBuilder)
    {
        _jsonSerializer = jsonSerializer;
        _publishedElementCache = publishedElementCache;
        _variationContextAccessor = variationContextAccessor;
        _apiElementBuilder = apiElementBuilder;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.ElementPicker.Equals(propertyType.EditorAlias);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<IPublishedElement>);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Elements;

    /// <inheritdoc />
    public override bool? IsValue(object? value, PropertyValueLevel level) =>
        value is not null && value.ToString() != "[]";

    /// <inheritdoc />
    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString()!;

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
        => GetElements(inter, preview);

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    /// <inheritdoc />
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    /// <inheritdoc />
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<IApiElement>);

    /// <inheritdoc />
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
    {
        IPublishedElement[]? elements = GetElements(inter, preview);
        return elements?.Select(_apiElementBuilder.Build);
    }

    private IPublishedElement[]? GetElements(object? inter, bool preview)
    {
        var value = inter as string;
        if (value.IsNullOrWhiteSpace())
        {
            return null;
        }

        Guid[]? keys = _jsonSerializer.Deserialize<Guid[]>(value);
        if (keys is null)
        {
            return null;
        }

        IEnumerable<IPublishedElement> elements = _publishedElementCache.GetByIds(preview, keys);

        if (preview is false && _variationContextAccessor.VariationContext?.Culture is not null)
        {
            elements = elements
                .Where(element => element.IsPublished(_variationContextAccessor.VariationContext.Culture));
        }

        return elements.ToArray();
    }
}
