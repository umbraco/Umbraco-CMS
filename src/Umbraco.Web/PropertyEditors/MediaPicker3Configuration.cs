using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using static Umbraco.Core.PropertyEditors.ValueConverters.ImageCropperValue;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the media picker value editor.
    /// </summary>
    public class MediaPicker3Configuration : IIgnoreUserStartNodesConfig
    {
        [ConfigurationField("filter", "Accepted types", "treesourcetypepicker",
            Description = "Limit to specific types")]
        public string Filter { get; set; }

        [ConfigurationField("multiple", "Pick multiple items", "boolean", Description = "Outputs a IEnumerable")]
        public bool Multiple { get; set; }

        [ConfigurationField("validationLimit", "Amount", "numberrange", Description = "Set a required range of medias")]
        public NumberRange ValidationLimit { get; set; } = new NumberRange();

        public class NumberRange
        {
            [JsonProperty("min")]
            public int? Min { get; set; }

            [JsonProperty("max")]
            public int? Max { get; set; }
        }

        [ConfigurationField("startNodeId", "Start node", "mediapicker")]
        public Udi StartNodeId { get; set; }

        [ConfigurationField(Core.Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes,
            "Ignore User Start Nodes", "boolean",
            Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
        public bool IgnoreUserStartNodes { get; set; }

        [ConfigurationField("enableLocalFocalPoint", "Enable Focal Point", "boolean")]
        public bool EnableLocalFocalPoint { get; set; }

        [ConfigurationField("crops", "Image Crops", "views/propertyeditors/MediaPicker3/prevalue/mediapicker3.crops.html", Description = "Local crops, stored on document")]
        public CropConfiguration[] Crops { get; set; }

        public class CropConfiguration
        {
            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }
        }
    }

    internal static class MediaPicker3ConfigurationExtensions
    {
        /// <summary>
        /// Applies the configuration to ensure only valid crops are kept and have the correct width/height.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public static void ApplyConfiguration(this ImageCropperValue imageCropperValue, MediaPicker3Configuration configuration)
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

            if (configuration?.EnableLocalFocalPoint == false)
            {
                imageCropperValue.FocalPoint = null;
            }
        }
    }
}
