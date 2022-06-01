// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Represents a value converter for the image cropper value editor.
/// </summary>
[DefaultPropertyValueConverter(typeof(JsonValueConverter))]
public class ImageCropperValueConverter : PropertyValueConverterBase
{
    private static readonly JsonSerializerSettings _imageCropperValueJsonSerializerSettings = new()
    {
        Culture = CultureInfo.InvariantCulture,
        FloatParseHandling = FloatParseHandling.Decimal,
    };

    private readonly ILogger<ImageCropperValueConverter> _logger;

    public ImageCropperValueConverter(ILogger<ImageCropperValueConverter> logger) => _logger = logger;

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
            value = JsonConvert.DeserializeObject<ImageCropperValue>(
                sourceString,
                _imageCropperValueJsonSerializerSettings);
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
}
