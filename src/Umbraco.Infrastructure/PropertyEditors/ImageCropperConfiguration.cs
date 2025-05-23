// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the image cropper value editor.
/// </summary>
public class ImageCropperConfiguration
{
    [ConfigurationField("crops")]
    public Crop[]? Crops { get; set; }

    public class Crop
    {
        public string Alias { get; set; } = null!;

        public int Width { get; set; }

        public int Height { get; set; }
    }
}

internal static class ImageCropperConfigurationExtensions
{
    /// <summary>
    ///     Applies the configuration to ensure only valid crops are kept and have the correct width/height.
    /// </summary>
    /// <param name="imageCropperValue"></param>
    /// <param name="configuration">The configuration.</param>
    public static void ApplyConfiguration(this ImageCropperValue imageCropperValue, ImageCropperConfiguration? configuration)
    {
        var crops = new List<ImageCropperValue.ImageCropperCrop>();

        ImageCropperConfiguration.Crop[]? configuredCrops = configuration?.Crops;
        if (configuredCrops != null)
        {
            foreach (ImageCropperConfiguration.Crop configuredCrop in configuredCrops)
            {
                ImageCropperValue.ImageCropperCrop? crop =
                    imageCropperValue.Crops?.FirstOrDefault(x => x.Alias == configuredCrop.Alias);

                crops.Add(new ImageCropperValue.ImageCropperCrop
                {
                    Alias = configuredCrop.Alias,
                    Width = configuredCrop.Width,
                    Height = configuredCrop.Height,
                    Coordinates = crop?.Coordinates,
                });
            }
        }

        imageCropperValue.Crops = crops;
    }
}
