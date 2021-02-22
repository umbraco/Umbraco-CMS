using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides extension methods for getting ImageProcessor URL from the core Image Cropper property editor
    /// </summary>
    public static class ImageCropperTemplateExtensions
    {
        internal static ImageCropperValue DeserializeImageCropperValue(this string json)
        {
            var imageCrops = new ImageCropperValue();
            if (json.DetectIsJson())
            {
                try
                {
                    imageCrops = JsonConvert.DeserializeObject<ImageCropperValue>(json, new JsonSerializerSettings
                    {
                        Culture = CultureInfo.InvariantCulture,
                        FloatParseHandling = FloatParseHandling.Decimal
                    });
                }
                catch (Exception ex)
                {
                    StaticApplicationLogging.Logger.LogError(ex, "Could not parse the json string: {Json}", json);
                }
            }

            return imageCrops;
        }
    }
}
