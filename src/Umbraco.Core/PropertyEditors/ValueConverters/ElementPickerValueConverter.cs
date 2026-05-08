using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal sealed class ElementPickerValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IPublishedElementCache _publishedElementCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IApiElementBuilder _apiElementBuilder;

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
        => GetElements(inter, preview);

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType)
        => GetPropertyCacheLevel(propertyType);

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Snapshot;

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(IEnumerable<IApiElement>);

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

        IEnumerable<IPublishedElement> elements = keys
            .Select(key => _publishedElementCache.GetByIdAsync(key, preview).GetAwaiter().GetResult())
            .WhereNotNull();

        if (preview is false && _variationContextAccessor.VariationContext?.Culture is not null)
        {
            elements = elements
                .Where(element => element.IsPublished(_variationContextAccessor.VariationContext.Culture));
        }

        return elements.ToArray();
    }
}
