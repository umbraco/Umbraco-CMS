using Newtonsoft.Json;

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
}
