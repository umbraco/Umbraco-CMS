// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Represents a value converter for the image cropper value editor.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class ImageCropperValueConverter : PropertyValueConverterBase, IDeliveryApiPropertyValueConverter
{
    private readonly IJsonSerializer _jsonSerializer;

    private readonly ILogger<ImageCropperValueConverter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.ValueConverters.ImageCropperValueConverter"/> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger{ImageCropperValueConverter}"/> used for logging.</param>
    /// <param name="jsonSerializer">An <see cref="IJsonSerializer"/> used to deserialize JSON data.</param>
    public ImageCropperValueConverter(ILogger<ImageCropperValueConverter> logger, IJsonSerializer jsonSerializer)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
    }

    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ImageCropper);

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => typeof(ImageCropperValue);

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        if (source == null)
        {
            return null;
        }

        var sourceString = source.ToString()!;

        ImageCropperValue? value;
        try
        {
            value = _jsonSerializer.Deserialize<ImageCropperValue>(sourceString);
        }
        catch (JsonException ex)
        {
            // Cannot deserialize, assume it may be a raw image URL.
            _logger.LogDebug(ex, "Could not deserialize string '{JsonString}' into an image cropper value.", sourceString);
            value = new ImageCropperValue { Src = sourceString };
        }

        value?.ApplyConfiguration(propertyType.DataType.ConfigurationAs<ImageCropperConfiguration>()!);

        return value;
    }

    /// <summary>
    /// Determines the appropriate <see cref="PropertyCacheLevel"/> to use for the delivery API
    /// based on the specified <see cref="IPublishedPropertyType"/>.
    /// </summary>
    /// <param name="propertyType">The published property type for which to determine the cache level.</param>
    /// <returns>The <see cref="PropertyCacheLevel"/> to be used for the delivery API.</returns>
    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    /// <summary>
    /// Returns the .NET type used by the Delivery API to represent the value of an image cropper property for the specified property type.
    /// </summary>
    /// <param name="propertyType">The published property type for which to determine the Delivery API value type.</param>
    /// <returns>The <see cref="Type"/> representing the Delivery API value for the image cropper property.</returns>
    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(ApiImageCropperValue);

    /// <summary>
    /// Converts an <see cref="ImageCropperValue"/> intermediate value into an <see cref="ApiImageCropperValue"/> suitable for the Delivery API.
    /// </summary>
    /// <param name="owner">The <see cref="IPublishedElement"/> that owns the property.</param>
    /// <param name="propertyType">The <see cref="IPublishedPropertyType"/> of the property being converted.</param>
    /// <param name="referenceCacheLevel">The cache level at which the reference is stored.</param>
    /// <param name="inter">The intermediate value to convert, expected to be an <see cref="ImageCropperValue"/>.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <param name="expanding">Indicates whether the value is being expanded for nested property conversion.</param>
    /// <returns>
    /// An <see cref="ApiImageCropperValue"/> representing the image cropper data, or <c>null</c> if the intermediate value is not a valid <see cref="ImageCropperValue"/>.
    /// </returns>
    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => inter is ImageCropperValue { Src: { } } imageCropperValue
            ? new ApiImageCropperValue(
                imageCropperValue.Src,
                imageCropperValue.GetImageFocalPoint(),
                imageCropperValue.GetImageCrops())
            : null;
}
