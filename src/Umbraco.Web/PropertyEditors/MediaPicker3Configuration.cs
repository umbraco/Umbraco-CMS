using Newtonsoft.Json;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

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

        [ConfigurationField("crops", "Image Croppings", "views/propertyeditors/MediaPicker3/prevalue/mediapicker3.crops.html", Description = "Local croppings, stored on document")]
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
}
