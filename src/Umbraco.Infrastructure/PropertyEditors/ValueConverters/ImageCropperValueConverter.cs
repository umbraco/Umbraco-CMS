// Copyright (c) Umbraco.
// See LICENSE for more details.

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
        catch (Exception ex)
        {
            // cannot deserialize, assume it may be a raw image URL
            _logger.LogError(ex, "Could not deserialize string '{JsonString}' into an image cropper value.", sourceString);
            value = new ImageCropperValue { Src = sourceString };
        }

        value?.ApplyConfiguration(propertyType.DataType.ConfigurationAs<ImageCropperConfiguration>()!);

        return value;
    }

    public PropertyCacheLevel GetDeliveryApiPropertyCacheLevel(IPublishedPropertyType propertyType) => GetPropertyCacheLevel(propertyType);

    public Type GetDeliveryApiPropertyValueType(IPublishedPropertyType propertyType) => typeof(ApiImageCropperValue);

    public object? ConvertIntermediateToDeliveryApiObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview, bool expanding)
        => inter is ImageCropperValue { Src: { } } imageCropperValue
            ? new ApiImageCropperValue(
                imageCropperValue.Src,
                imageCropperValue.GetImageFocalPoint(),
                imageCropperValue.GetImageCrops())
            : null;
}
