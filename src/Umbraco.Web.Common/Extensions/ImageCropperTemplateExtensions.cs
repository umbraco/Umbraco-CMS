using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for getting ImageProcessor URL from the core Image Cropper property editor
/// </summary>
public static class ImageCropperTemplateExtensions
{
    internal static ImageCropperValue DeserializeImageCropperValue(this string json)
    {
        ImageCropperValue? imageCrops = null;

        if (json.DetectIsJson())
        {
            try
            {
                IJsonSerializer? serializer = StaticServiceProvider.Instance.GetService<IJsonSerializer>();
                imageCrops = serializer?.Deserialize<ImageCropperValue>(json);
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
