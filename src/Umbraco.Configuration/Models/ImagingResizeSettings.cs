using System.Text.Json.Serialization;

namespace Umbraco.Configuration.Models
{
    public class ImagingResizeSettings
    {
        [JsonPropertyName("MaxWidth")]
        public int MaxResizeWidth { get; set; } = 5000;

        [JsonPropertyName("MaxHeight")]
        public int MaxResizeHeight { get; set; } = 5000;
    }
}
