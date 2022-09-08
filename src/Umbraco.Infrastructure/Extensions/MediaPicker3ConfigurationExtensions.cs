using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Extensions;

public static class MediaPicker3ConfigurationExtensions
{
    /// <summary>
    ///     Applies the configuration to ensure only valid crops are kept and have the correct width/height.
    /// </summary>
    public static void ApplyConfiguration(this ImageCropperValue imageCropperValue, MediaPicker3Configuration? configuration)
    {
        var crops = new List<ImageCropperValue.ImageCropperCrop>();

        MediaPicker3Configuration.CropConfiguration[]? configuredCrops = configuration?.Crops;
        if (configuredCrops != null)
        {
            foreach (MediaPicker3Configuration.CropConfiguration configuredCrop in configuredCrops)
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
