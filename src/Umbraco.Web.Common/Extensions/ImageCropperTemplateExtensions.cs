using System.Text.Json;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for getting ImageProcessor URL from the core Image Cropper property editor
/// </summary>
public static class ImageCropperTemplateExtensions
{
    private static readonly JsonSerializerOptions _imageCropperValueJsonSerializerSettings = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal static ImageCropperValue DeserializeImageCropperValue(this string json)
    {
        ImageCropperValue? imageCrops = null;

        if (json.DetectIsJson())
        {
            try
            {
                imageCrops =
                    JsonSerializer.Deserialize<ImageCropperValue>(json, _imageCropperValueJsonSerializerSettings);
            }
            catch (Exception ex)
            {
                StaticApplicationLogging.Logger.LogError(ex, "Could not parse the json string: {Json}", json);
            }
        }

        imageCrops ??= new ImageCropperValue();
        return imageCrops;
    }
}
