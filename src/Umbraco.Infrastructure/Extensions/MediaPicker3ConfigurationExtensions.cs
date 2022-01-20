﻿using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Extensions
{
    public static class MediaPicker3ConfigurationExtensions
    {
        /// <summary>
        /// Applies the configuration to ensure only valid crops are kept and have the correct width/height.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public static void ApplyConfiguration(this ImageCropperValue imageCropperValue, MediaPicker3Configuration configuration)
        {
            var crops = new List<ImageCropperValue.ImageCropperCrop>();

            var configuredCrops = configuration?.Crops;
            if (configuredCrops != null)
            {
                foreach (var configuredCrop in configuredCrops)
                {
                    var crop = imageCropperValue.Crops?.FirstOrDefault(x => x.Alias == configuredCrop.Alias);

                    crops.Add(new ImageCropperValue.ImageCropperCrop
                    {
                        Alias = configuredCrop.Alias,
                        Width = configuredCrop.Width,
                        Height = configuredCrop.Height,
                        Coordinates = crop?.Coordinates
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
}
