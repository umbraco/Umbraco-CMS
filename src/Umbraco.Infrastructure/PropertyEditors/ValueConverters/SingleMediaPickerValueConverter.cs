using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.DeliveryApi;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Converts the value stored by the single media picker property editor into a strongly-typed object representing
/// the selected media item and its associated crop data, making it accessible for use in code.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class SingleMediaPickerValueConverter : MediaPickerWithCropsValueConverterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleMediaPickerValueConverter"/> class.
    /// </summary>
    /// <param name="publishedMediaCache">Provides access to the cache of published media items.</param>
    /// <param name="publishedUrlProvider">Resolves URLs for published content and media.</param>
    /// <param name="publishedValueFallback">Handles fallback logic for published property values.</param>
    /// <param name="jsonSerializer">Serializes and deserializes JSON data for property values.</param>
    /// <param name="apiMediaWithCropsBuilder">Builds API representations of media items with crop data.</param>
    public SingleMediaPickerValueConverter(
        IPublishedMediaCache publishedMediaCache,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedValueFallback publishedValueFallback,
        IJsonSerializer jsonSerializer,
        IApiMediaWithCropsBuilder apiMediaWithCropsBuilder)
        : base(publishedMediaCache, publishedUrlProvider, publishedValueFallback, jsonSerializer, apiMediaWithCropsBuilder)
    {
    }

    /// <inheritdoc />
    protected override bool HoldsMultipleItems => false;

    /// <summary>
    /// Determines whether this converter applies to the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type to check.</param>
    /// <returns><c>true</c> if this converter is applicable; otherwise, <c>false</c>.</returns>
    public override bool IsConverter(IPublishedPropertyType propertyType) =>
        propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.SingleMediaPicker);

    /// <summary>
    /// Determines the CLR type returned for the property value.
    /// </summary>
    /// <param name="propertyType">The published property type to inspect.</param>
    /// <returns><see cref="MediaWithCrops" />.</returns>
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(MediaWithCrops);
}
