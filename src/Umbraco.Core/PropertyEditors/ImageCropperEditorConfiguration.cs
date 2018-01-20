using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    public class ImageCropperEditorConfiguration
    {
        [JsonProperty("crops")]
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
