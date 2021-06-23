using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.PropertyEditors.ValueConverters;
using static Umbraco.Core.PropertyEditors.ValueConverters.ImageCropperValue;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the image cropper value editor.
    /// </summary>
    public class ImageCropperConfiguration
    {
        [ConfigurationField("crops", "Define crops", "views/propertyeditors/imagecropper/imagecropper.prevalues.html")]
        public Crop[] Crops { get; set; }

        public class Crop
        {
            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }
        }
    }

    internal static class ImageCropperConfigurationExtensions
    {
        /// <summary>
        /// Applies the configuration to ensure only valid crops are kept and have the correct width/height.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public static void ApplyConfiguration(this ImageCropperValue imageCropperValue, ImageCropperConfiguration configuration)
        {
            var crops = new List<ImageCropperCrop>();

            var configuredCrops = configuration?.Crops;
            if (configuredCrops != null)
            {
                foreach (var configuredCrop in configuredCrops)
                {
                    var crop = imageCropperValue.Crops?.FirstOrDefault(x => x.Alias == configuredCrop.Alias);

                    crops.Add(new ImageCropperCrop
                    {
                        Alias = configuredCrop.Alias,
                        Width = configuredCrop.Width,
                        Height = configuredCrop.Height,
                        Coordinates = crop?.Coordinates
                    });
                }
            }

            imageCropperValue.Crops = crops;
        }
    }
}
