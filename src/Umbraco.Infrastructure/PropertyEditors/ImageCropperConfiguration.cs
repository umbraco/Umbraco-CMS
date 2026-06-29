// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the image cropper value editor.
/// </summary>
public class ImageCropperConfiguration
{
    /// <summary>
    /// Gets or sets the collection of crop definitions used by the image cropper configuration.
    /// </summary>
    [ConfigurationField("crops")]
    public Crop[]? Crops { get; set; }

    /// <summary>
    /// Defines the settings for a specific crop within the image cropper configuration.
    /// </summary>
    public class Crop
    {
        /// <summary>
        /// Gets or sets the unique alias that identifies this crop configuration.
        /// </summary>
        public string Alias { get; set; } = null!;

        /// <summary>
        /// Gets or sets the width of the crop area.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the crop, in pixels.
        /// </summary>
        public int Height { get; set; }
    }
}

internal static class ImageCropperConfigurationExtensions
{
    /// <summary>
    ///     Applies the configuration to ensure only valid crops are kept and have the correct width/height.
    /// </summary>
    /// <param name="imageCropperValue">The image cropper value to apply configuration to.</param>
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
