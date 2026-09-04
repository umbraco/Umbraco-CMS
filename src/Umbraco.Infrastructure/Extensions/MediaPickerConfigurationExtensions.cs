using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for configuring and working with media picker settings in Umbraco.
/// </summary>
public static class MediaPickerConfigurationExtensions
{
    /// <summary>
    ///     Applies the configuration to ensure only valid crops are kept and have the correct width/height.
    /// </summary>
    public static void ApplyConfiguration(this ImageCropperValue imageCropperValue, MediaPickerConfigurationBase? configuration)
    {
        var crops = new List<ImageCropperValue.ImageCropperCrop>();

        MediaPickerConfigurationBase.CropConfiguration[]? configuredCrops = configuration?.Crops;
        if (configuredCrops != null)
        {
            foreach (MediaPickerConfigurationBase.CropConfiguration configuredCrop in configuredCrops)
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

        if (configuration?.EnableLocalFocalPoint == false)
        {
            imageCropperValue.FocalPoint = null;
        }
    }
}
