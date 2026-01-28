using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public class ElementPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IElementCacheService _elementCacheService;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public ElementPickerValueConverter(IJsonSerializer jsonSerializer, IElementCacheService elementCacheService, IVariationContextAccessor variationContextAccessor)
    {
        _jsonSerializer = jsonSerializer;
        _elementCacheService = elementCacheService;
        _variationContextAccessor = variationContextAccessor;
    }

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.ElementPicker.Equals(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(IEnumerable<IPublishedElement>);

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Elements;

    public override bool? IsValue(object? value, PropertyValueLevel level) =>
        value is not null && value.ToString() != "[]";

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => source?.ToString()!;

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
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

        IEnumerable<IPublishedElement> elements = keys
            .Select(key => _elementCacheService.GetByKeyAsync(key, preview).GetAwaiter().GetResult())
            .WhereNotNull();

        if (preview is false && _variationContextAccessor.VariationContext?.Culture is not null)
        {
            elements = elements
                .Where(element => element.IsPublished(_variationContextAccessor.VariationContext.Culture));
        }

        return elements.ToArray();
    }

    // TODO ELEMENTS: implement Delivery API
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    // TODO ELEMENTS: implement Delivery API
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType)
        => GetPropertyValueType(propertyType);

    // TODO ELEMENTS: implement Delivery API
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => null;
}
