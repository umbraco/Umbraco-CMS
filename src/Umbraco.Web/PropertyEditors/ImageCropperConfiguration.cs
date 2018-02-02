using Newtonsoft.Json;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the image cropper value editor.
    /// </summary>
    public class ImageCropperConfiguration
    {
        [ConfigurationField("crops", "Crop sizes", "views/propertyeditors/imagecropper/imagecropper.prevalues.html")]
        public ImageCropperCrop[] Crops { get; set; }

        public class ImageCropperCrop
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