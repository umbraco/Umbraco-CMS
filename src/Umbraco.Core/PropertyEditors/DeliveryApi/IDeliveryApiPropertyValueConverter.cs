using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.DeliveryApi;

public interface IDeliveryApiPropertyValueConverter : IPropertyValueConverter
{
    /// <summary>
    ///     Gets the property cache level for Delivery API representation.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The property cache level.</returns>
    PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType);

    /// <summary>
    ///     Gets the property cache level for Delivery API representation when expanding the property.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The property cache level.</returns>
    /// <remarks>Defaults to the value of <see cref="GetDeliveryApiPropertyCacheLevel"/>.</remarks>
    PropertyCacheLevel GetDeliveryApiPropertyCacheLevelForExpansion(IPublishedPropertyType propertyType)
        => GetDeliveryApiPropertyCacheLevel(propertyType);

    /// <summary>
    ///     Gets the type of values returned by the converter for Delivery API representation.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The CLR type of values returned by the converter.</returns>
    /// <remarks>
    ///     Some of the CLR types may be generated, therefore this method cannot directly return
    ///     a Type object (which may not exist yet). In which case it needs to return a ModelType instance.
    /// </remarks>
    Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType);

    /// <summary>
    ///     Converts a property intermediate value to an Object value for Delivery API representation.
    /// </summary>
    /// <param name="owner">The property set owning the property.</param>
    /// <param name="propertyType">The property type.</param>
    /// <param name="referenceCacheLevel">The reference cache level.</param>
    /// <param name="inter">The intermediate value.</param>
    /// <param name="preview">A value indicating whether conversion should take place in preview mode.</param>
    /// <param name="expanding">A value indicating whether the property value should be expanded (if applicable).</param>
    /// <returns>The result of the conversion.</returns>
    /// <remarks>
    ///     <para>
    ///         The converter should know how to convert a <c>null</c> intermediate value, or any intermediate value
    ///         indicating that no value has been assigned to the property. It is up to the converter to determine
    ///         what to return in that case: either <c>null</c>, or the default value...
    ///     </para>
    ///     <para>
    ///         The <paramref name="referenceCacheLevel" /> is passed to the converter so that it can be, in turn,
    ///         passed to eg a PublishedFragment constructor. It is used by the fragment and the properties to manage
    ///         the cache levels of property values. It is not meant to be used by the converter.
    ///     </para>
    /// </remarks>
    object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding);

    [Obsolete($"Use the {nameof(ConvertIntermediateToDeliveryApiObject)} that supports property expansion. Will be removed in V14.")]
    object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
            => ConvertIntermediateToDeliveryApiObject(owner, propertyType, referenceCacheLevel, inter, preview, false);
}
